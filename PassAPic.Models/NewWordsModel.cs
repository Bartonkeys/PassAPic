namespace PassAPic.Models
{
    public class NewWordsModel
    {
        [Required]
        public string Words { get; set; }

        [Required]
        public string Password { get; set; }

        //[Required]
        //[Range(0, 1)]
        //public int Mode { get; set; }

        [Required]
        public Mode Mode { get; set; }
    }
}