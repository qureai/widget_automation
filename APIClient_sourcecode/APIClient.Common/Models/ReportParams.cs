using APIClient.Common.Enums;

namespace APIClient.Common.Models
{
    public class ReportParams
    {
        //
        // Summary:
        //     The name of the site that the accessions belong to.
        public string SiteName {  get; set; }

        //
        // Summary:
        //     The filler order numbers (accessions) associated with the report.
        public string[] AccessionNumbers { get; set; }

        //
        // Summary:
        //     The patient identifier (MRN or Dept. Number) associated with the report.
        public string PatientIdentifier { get; set; }

        //
        // Summary:
        //     The report status taken from the Nuance.RadWhere.RadWhereReportStatus enumeration.
        public ReportStatus Status { get; set; }

        //
        // Summary:
        //     Specifies whether the report is an addendum.
        public bool IsAddendum { get; set; }

        //
        // Summary:
        //     The report text w/o any formatting.
        public string PlainText { get; set; }

        //
        // Summary:
        //     The report text formatted in RTF.
        public string RichText { get; set; }

        //
        // Summary:
        //     Indicates whether the report reached a preliminary status like Corrected or PendingSignature,
        //     and the user requested to be sent to the RIS as a 'P' (preliminary) result.
        public bool SentAsPreliminary { get; set; }

        //
        // Summary:
        //     The signer username (Attending radiologist) of the report.
        public string SignerUsername { get; set; }

        //
        // Summary:
        //     The signer site identifier (Attending radiologist) of the report.
        public string SignerSiteIdentifier { get; set; }

        //
        // Summary:
        //     The dictator username (Resident radiologist) of the report.
        public string DictatorUsername { get; set; }

        //
        // Summary:
        //     The dictator site identifier (Resident radiologist) of the report.
        public string DictatorSiteIdentifier { get; set; }

        //
        // Summary:
        //     The report final status taken from the Nuance.RadWhere.RadWhereReportStatus enumeration.
        public ReportStatus ImpendingStatus { get; set; }
    }
}
