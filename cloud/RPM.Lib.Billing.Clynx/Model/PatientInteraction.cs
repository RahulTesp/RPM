

namespace RPMPatientBilling.Model
{
    public class PatientInteraction
    {
        public int Duration { get; set; }

        public DateTime Date { get; set; }  

        public int IsCallNote { get; set; }

        public int IsEstablishedCall { get; set; }
    }

    public class PatientInteractionUI
    {
        public int PatientProgramId { get; set; }
        public int Duration { get; set; }

        public DateTime Date { get; set; }

        public int IsCallNote { get; set; }

        public int IsEstablishedCall { get; set; }
    }
}
