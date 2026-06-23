namespace APIClient.Common.Enums
{
    public enum ReportStatus
    {
        //
        // Summary:
        //     Specifies that a report was created and then discarded.
        Discarded = -1,
        //
        // Summary:
        //     Specifies that the status is unknown or pending because the report is currently
        //     being saved by the application in the background.
        Pending = 0,
        //
        // Summary:
        //     Specifies that the report status is WetRead.
        WetRead = 1,
        //
        // Summary:
        //     Specifies that the report status is Draft.
        Draft = 2,
        //
        // Summary:
        //     Specifies that the report status is PendingCorrection. The report enters that
        //     status when its author sends it to an editor for correction/transcription.
        PendingCorrection = 3,
        //
        // Summary:
        //     Specifies that the report status is Corrected. The report enters that status
        //     when an editor is done correcting/transcribing it.
        Corrected = 4,
        //
        // Summary:
        //     Specifies that the report status is CorrectionRejected. The report enters that
        //     status when the author rejects the report corrected/transcribed by an editor.
        CorrectionRejected = 5,
        //
        // Summary:
        //     Specifies that the report status is PendingSignature. The report enters this
        //     status when approved by the resident who dictated it, or when signed as preliminary
        //     by the attending.
        PendingSignature = 6,
        //
        // Summary:
        //     Specifies that the report status is SignRejected. The report enters this status
        //     when the attending rejects the report approved by the resident.
        SignRejected = 7,
        //
        // Summary:
        //     Specifies that the report status is Final.
        Final = 8
    }
}
