using TinyMVVM;

namespace Organilog.Models
{
    public class InterventionHistory : BaseModel
    {
        private string isDone;
        public string IsDone { get => isDone; set => SetProperty(ref isDone, value); }

        private string date;
        public string Date { get => date; set => SetProperty(ref date, value); }

        private string comment;
        public string Comment { get => comment; set => SetProperty(ref comment, value); }
    }
}