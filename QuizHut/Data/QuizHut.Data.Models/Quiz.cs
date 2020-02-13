﻿namespace QuizHut.Data.Models
{
    using System;
    using System.Collections.Generic;

    using QuizHut.Data.Common.Models;

    public class Quiz : BaseDeletableModel<string>
    {
        public Quiz()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Questions = new HashSet<Question>();
            this.QuizResults = new HashSet<QuizResult>();
            this.QuizzesGroups = new HashSet<QuizGroup>();
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Password { get; set; }

        public int? Duration { get; set; }

        public DateTime? ActivationDateAndTime { get; set; }

        public string CreatorId { get; set; }

        public virtual ApplicationUser Creator { get; set; }

        public string CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<QuizGroup> QuizzesGroups { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public virtual ICollection<QuizResult> QuizResults { get; set; }
    }
}
