using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace olympo_webapi.Models
{
    public class Exercise
    {
        internal readonly int Repetitions;
        internal readonly int Series;
        internal readonly double Breaks;
        internal readonly double Time;

        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }

        [NotMapped]
        public IFormFile? Video { get; set; }

        [JsonIgnore]
        public ICollection<UserExercise>? Users { get; set; } = new List<UserExercise>();
        
        [JsonIgnore]
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    }
}
