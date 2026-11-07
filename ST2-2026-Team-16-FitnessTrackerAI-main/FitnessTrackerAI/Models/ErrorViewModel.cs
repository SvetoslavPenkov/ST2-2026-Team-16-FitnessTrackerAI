namespace FitnessTrackerAI.Models
{
    public class ErrorViewModel
    {
        //Служебен модел, съдържащ метаданни за възникнали грешки (като RequestId), които се предават към изгледа за грешки.
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
