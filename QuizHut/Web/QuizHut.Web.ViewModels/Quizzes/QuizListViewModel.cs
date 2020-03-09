﻿namespace QuizHut.Web.ViewModels.Quizzes
{
    using AutoMapper;
    using QuizHut.Data.Models;
    using QuizHut.Services.Mapping;
    using QuizHut.Web.ViewModels.Common;

    public class QuizListViewModel : IMapFrom<Quiz>, IHaveCustomMappings
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int QuestionsCount { get; set; }

        public string CreatedOn { get; set; }

        public bool IsActive { get; set; }

        public string Color { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Quiz, QuizListViewModel>()
                .ForMember(
                    x => x.CreatedOn,
                    opt => opt.MapFrom(x => x.CreatedOn.ToString("dd/MM/yyyy")))
                .ForMember(
                    x => x.QuestionsCount,
                    opt => opt.MapFrom(x => x.Questions.Count))
                .ForMember(
                    x => x.Color,
                    opt => opt.MapFrom(x => x.IsActive ? ModelCostants.ColorActive : ModelCostants.ColorEnded));
        }
    }
}
