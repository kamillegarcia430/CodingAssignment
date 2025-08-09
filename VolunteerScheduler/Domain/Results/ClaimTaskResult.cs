namespace VolunteerScheduler.Domain.Results
{
    public class ClaimTaskResult
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public ClaimTaskStatus Status { get; private set; }

        private ClaimTaskResult(bool isSuccess, ClaimTaskStatus status, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            Status = status;
            ErrorMessage = errorMessage;
        }

        public static ClaimTaskResult Success() =>
            new ClaimTaskResult(true, ClaimTaskStatus.Success);

        public static ClaimTaskResult Failure(ClaimTaskStatus status, string errorMessage) =>
            new ClaimTaskResult(false, status, errorMessage);
    }

    public enum ClaimTaskStatus
    {
        Success,
        TaskNotFound,
        TaskFullyBooked,
        AlreadyClaimed,
        ParentNotFound,
        Error,
        OverlappingTask
    }
}
