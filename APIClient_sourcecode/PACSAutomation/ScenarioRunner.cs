using System;
using System.Collections.Generic;
using System.Threading;

namespace PACSAutomation
{
    // ─── Scenario Result ─────────────────────────────────────────────────────────

    public enum ScenarioStatus { NotRun, Running, Passed, Failed, Skipped }

    public class ScenarioResult
    {
        public string        ScenarioName { get; set; }
        public ScenarioStatus Status       { get; set; } = ScenarioStatus.NotRun;
        public string        Detail       { get; set; } = string.Empty;
        public TimeSpan      Duration     { get; set; }
    }

    // ─── Scenario Definition ─────────────────────────────────────────────────────

    public class AutomationScenario
    {
        public string Id          { get; set; }
        public string Name        { get; set; }
        public string Description { get; set; }
        public string AccessionNumbers { get; set; }
        public string SiteName    { get; set; } = "";   // empty = system-wide search
        public string MRN         { get; set; } = "";

        /// <summary>The action this scenario will execute.</summary>
        public Func<PowerScribeAutomation, AutomationScenario, ScenarioResult> Execute { get; set; }
    }

    // ─── Scenario Runner ─────────────────────────────────────────────────────────

    /// <summary>
    /// Builds and runs the PACS automation scenario suite against a
    /// connected + logged-in <see cref="PowerScribeAutomation"/> instance.
    /// </summary>
    public static class ScenarioRunner
    {
        // Delay between actions so PS can process them visually
        private const int StepDelayMs = 1500;

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>Runs a single scenario and returns its result.</summary>
        public static ScenarioResult Run(AutomationScenario scenario, PowerScribeAutomation ps)
        {
            var result = new ScenarioResult { ScenarioName = scenario.Name, Status = ScenarioStatus.Running };
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                result = scenario.Execute(ps, scenario);
                result.ScenarioName = scenario.Name;
            }
            catch (Exception ex)
            {
                result.Status = ScenarioStatus.Failed;
                result.Detail = ex.Message;
            }
            sw.Stop();
            result.Duration = sw.Elapsed;
            return result;
        }

        // ── Scenario Catalogue ───────────────────────────────────────────────────

        /// <summary>
        /// Returns the full list of demo-PACS scenarios, ready to run.
        /// Fill in real accession numbers and site names before executing.
        /// </summary>
        public static List<AutomationScenario> BuildScenarios(ScenarioConfig cfg) =>
            new List<AutomationScenario>
            {
                // ── 1 ─────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S01",
                    Name        = "Open New Draft Report → Close Without Signing",
                    Description = "Opens a brand-new accession (creates a draft), then closes it "
                                + "without signing. Report stays in Draft status.",
                    AccessionNumbers = cfg.AccessionNew,
                    SiteName    = cfg.SiteName,

                    Execute = (ps, s) =>
                    {
                        // Open
                        bool opened = ps.OpenReport(s.SiteName, s.AccessionNumbers);
                        if (!opened) return Fail("OpenReport returned false. Verify accession exists.");

                        Sleep();

                        // Close without signing
                        bool closed = ps.CloseReport(sign: false, preliminary: false);
                        if (!closed) return Fail("CloseReport (no sign) returned false.");

                        return Pass("Draft report opened and closed successfully.");
                    }
                },

                // ── 2 ─────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S02",
                    Name        = "Open Existing Report → Sign as Preliminary",
                    Description = "Opens an existing draft report and signs it as preliminary "
                                + "(PendingSignature). Typical for residents or attending prelim sign.",
                    AccessionNumbers = cfg.AccessionExisting,
                    SiteName    = cfg.SiteName,

                    Execute = (ps, s) =>
                    {
                        bool opened = ps.OpenReport(s.SiteName, s.AccessionNumbers);
                        if (!opened) return Fail("OpenReport returned false.");

                        Sleep();

                        // sign=true, preliminary=true → PendingSignature status
                        bool closed = ps.CloseReport(sign: true, preliminary: true);
                        if (!closed) return Fail("CloseReport (prelim sign) returned false or user cancelled.");

                        return Pass("Report signed as preliminary (PendingSignature).");
                    }
                },

                // ── 3 ─────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S03",
                    Name        = "Open Existing Report → Sign as Final",
                    Description = "Opens a report in PendingSignature state and signs it as final. "
                                + "Requires attending-level user.",
                    AccessionNumbers = cfg.AccessionPendingSignature,
                    SiteName    = cfg.SiteName,

                    Execute = (ps, s) =>
                    {
                        bool opened = ps.OpenReport(s.SiteName, s.AccessionNumbers);
                        if (!opened) return Fail("OpenReport returned false.");

                        Sleep();

                        // sign=true, preliminary=false → Final status
                        bool closed = ps.CloseReport(sign: true, preliminary: false);
                        if (!closed) return Fail("CloseReport (final sign) returned false or user cancelled.");

                        return Pass("Report signed as Final.");
                    }
                },

                // ── 4 ─────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S04",
                    Name        = "Open Report → Save → Close",
                    Description = "Opens a report, saves it (status unchanged), then closes it "
                                + "without changing status.",
                    AccessionNumbers = cfg.AccessionNew,
                    SiteName    = cfg.SiteName,

                    Execute = (ps, s) =>
                    {
                        bool opened = ps.OpenReport(s.SiteName, s.AccessionNumbers);
                        if (!opened) return Fail("OpenReport returned false.");

                        Sleep();

                        // Save without closing
                        bool saved = ps.SaveReport(closeAfterSave: false);
                        if (!saved) return Fail("SaveReport returned false.");

                        Sleep();

                        bool closed = ps.CloseReport(sign: false, preliminary: false);
                        if (!closed) return Fail("CloseReport returned false.");

                        return Pass("Report saved and closed successfully.");
                    }
                },

                // ── 5 ─────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S05",
                    Name        = "Open Report → Cancel / Discard",
                    Description = "Opens a report and then cancels it. "
                                + "With discard=true the report is deleted entirely.",
                    AccessionNumbers = cfg.AccessionDiscard,
                    SiteName    = cfg.SiteName,

                    Execute = (ps, s) =>
                    {
                        bool opened = ps.OpenReport(s.SiteName, s.AccessionNumbers);
                        if (!opened) return Fail("OpenReport returned false.");

                        Sleep();

                        // discard=false → close without saving (report stays, no delete)
                        // discard=true  → permanently delete the report
                        bool cancelled = ps.CancelReport(discard: false);
                        if (!cancelled) return Fail("CancelReport returned false (modal dialog may be blocking).");

                        return Pass("Report cancelled (not discarded).");
                    }
                },

                // ── 6 ─────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S06",
                    Name        = "Open Report → Associate Additional Orders",
                    Description = "Opens a report and associates an extra accession with it "
                                + "(multi-order report). Then dissociates the extra order.",
                    AccessionNumbers = cfg.AccessionNew,
                    SiteName    = cfg.SiteName,

                    Execute = (ps, s) =>
                    {
                        bool opened = ps.OpenReport(s.SiteName, s.AccessionNumbers);
                        if (!opened) return Fail("OpenReport returned false.");

                        Sleep();

                        // Associate an additional accession
                        int associated = ps.AssociateOrders(cfg.AccessionExtra);
                        if (associated <= 0 && associated != -1)  // -1 means already associated
                            return Fail($"AssociateOrders returned {associated}. Check accession '{cfg.AccessionExtra}'.");

                        Sleep();

                        // Dissociate the extra accession again
                        bool dissociated = ps.DissociateOrders(cfg.AccessionExtra);
                        if (!dissociated) return Fail("DissociateOrders returned false.");

                        Sleep();

                        bool closed = ps.CloseReport(sign: false);
                        return closed ? Pass("Associate/Dissociate cycle completed.") : Fail("CloseReport failed.");
                    }
                },

                // ── 7 ─────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S07",
                    Name        = "Open Report with MRN (OpenReportEx)",
                    Description = "Opens a report filtering by both accession number and patient MRN. "
                                + "Useful when the same accession may appear on multiple sites.",
                    AccessionNumbers = cfg.AccessionExisting,
                    SiteName    = cfg.SiteName,
                    MRN         = cfg.PatientMRN,

                    Execute = (ps, s) =>
                    {
                        if (string.IsNullOrWhiteSpace(s.MRN))
                            return Skip("MRN not configured. Set PatientMRN in ScenarioConfig.");

                        bool opened = ps.OpenReportEx(s.SiteName, s.AccessionNumbers, s.MRN);
                        if (!opened) return Fail("OpenReportEx returned false.");

                        Sleep();

                        bool closed = ps.CloseReport(sign: false);
                        return closed ? Pass($"OpenReportEx with MRN '{s.MRN}' succeeded.") : Fail("CloseReport failed.");
                    }
                },

                // ── 8 ─────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S08",
                    Name        = "Preview Orders (No Report Opened)",
                    Description = "Searches and previews an order in the Explorer window. "
                                + "Does NOT open the report for editing.",
                    AccessionNumbers = cfg.AccessionExisting,
                    SiteName    = cfg.SiteName,

                    Execute = (ps, s) =>
                    {
                        bool success = ps.PreviewOrders(s.SiteName, s.AccessionNumbers);
                        return success ? Pass("PreviewOrders succeeded.") : Fail("PreviewOrders failed — accessions not found.");
                    }
                },

                // ── 9 ─────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S09",
                    Name        = "Open Multi-Accession Report",
                    Description = "Opens a single report linked to two accession numbers "
                                + "(e.g. a CT and a scout). Accessions are comma-separated.",
                    AccessionNumbers = cfg.AccessionMulti,
                    SiteName    = cfg.SiteName,

                    Execute = (ps, s) =>
                    {
                        if (!s.AccessionNumbers.Contains(","))
                            return Skip("AccessionMulti must contain at least two comma-separated values.");

                        bool opened = ps.OpenReport(s.SiteName, s.AccessionNumbers);
                        if (!opened) return Fail("OpenReport (multi-accession) returned false.");

                        Sleep();

                        bool closed = ps.CloseReport(sign: false);
                        return closed ? Pass("Multi-accession report opened and closed.") : Fail("CloseReport failed.");
                    }
                },

                // ── 10 ────────────────────────────────────────────────────────────
                new AutomationScenario
                {
                    Id          = "S10",
                    Name        = "AssociateOrdersEx (from Explorer Screen)",
                    Description = "While in Explorer screen, replaces the accession list of an "
                                + "existing report using AssociateOrdersEx.",
                    AccessionNumbers = cfg.AccessionExisting,
                    SiteName    = cfg.SiteName,

                    Execute = (ps, s) =>
                    {
                        if (string.IsNullOrWhiteSpace(cfg.AccessionExtra))
                            return Skip("AccessionExtra not configured.");

                        // AssociateOrdersEx runs from Explorer (no report needs to be open)
                        // newAccessionNumbers is the AUTHORITATIVE list — extras will be removed
                        int affected = ps.AssociateOrdersEx(
                            siteName:             s.SiteName,
                            currentAccessionNumber: s.AccessionNumbers,
                            newAccessionNumbers:    $"{s.AccessionNumbers},{cfg.AccessionExtra}");

                        return affected >= 0
                            ? Pass($"AssociateOrdersEx: {affected} order(s) affected.")
                            : Fail("AssociateOrdersEx returned -1 (modal dialog was active).");
                    }
                },
            };

        // ── Result Helpers ────────────────────────────────────────────────────────

        private static ScenarioResult Pass(string detail)   => new ScenarioResult { Status = ScenarioStatus.Passed,  Detail = detail };
        private static ScenarioResult Fail(string detail)   => new ScenarioResult { Status = ScenarioStatus.Failed,  Detail = detail };
        private static ScenarioResult Skip(string detail)   => new ScenarioResult { Status = ScenarioStatus.Skipped, Detail = detail };
        private static void           Sleep()               => Thread.Sleep(StepDelayMs);
    }

    // ─── Config ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Holds the accession numbers and site info for the scenario suite.
    /// Edit these to match your PowerScribe test environment.
    /// </summary>
    public class ScenarioConfig
    {
        // ── Connection ────────────────────────────────────────────────────────────
        public string Username { get; set; } = "raduser";
        public string Password { get; set; } = "password";

        /// <summary>
        /// Site name as configured in PowerScribe.
        /// Leave empty ("") for system-wide search across all accessible sites.
        /// </summary>
        public string SiteName { get; set; } = "";

        // ── Accession Numbers ─────────────────────────────────────────────────────

        /// <summary>A brand-new accession (no existing report). Used to test new report creation.</summary>
        public string AccessionNew { get; set; } = "ACC001";

        /// <summary>An accession that already has a draft report.</summary>
        public string AccessionExisting { get; set; } = "ACC002";

        /// <summary>An accession with a report in PendingSignature state (for final sign test).</summary>
        public string AccessionPendingSignature { get; set; } = "ACC003";

        /// <summary>An accession to use for the Cancel/Discard test.</summary>
        public string AccessionDiscard { get; set; } = "ACC004";

        /// <summary>A secondary accession for associate/dissociate tests.</summary>
        public string AccessionExtra { get; set; } = "ACC005";

        /// <summary>Comma-separated list of ≥2 accessions for the multi-order test.</summary>
        public string AccessionMulti { get; set; } = "ACC006,ACC007";

        /// <summary>Patient MRN for the OpenReportEx test. Leave empty to skip.</summary>
        public string PatientMRN { get; set; } = "";
    }
}
