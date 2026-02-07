namespace Apps.Taus.Constants;

public static class ApiEndpoints
{
    public const string Estimate = "/1.0/estimate";
    public const string EstimateV2 = "/2.0/estimate";
    public const string EstimateFileUpload = "/2.0/estimate/file/upload";
    public const string EstimateBatchJob = "/2.0/estimate-batch/job";
    public const string ListBatchJobs = "/2.0/estimate-batch/job/";
    public const string BatchJobDownload = "/2.0/estimate-batch/job/{job_id}/download";
}
