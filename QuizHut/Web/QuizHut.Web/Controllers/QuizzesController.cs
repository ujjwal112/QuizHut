﻿namespace QuizHut.Web.Controllers
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using QuizHut.Common;
    using QuizHut.Data.Models;
    using QuizHut.Services.EventsResults;
    using QuizHut.Services.Quizzes;
    using QuizHut.Web.Common;
    using QuizHut.Web.Filters;
    using QuizHut.Web.ViewModels.Quizzes;

    public class QuizzesController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IQuizzesService quizService;
        private readonly IEventsResultsService quizResultService;

        public QuizzesController(
            UserManager<ApplicationUser> userManager,
            IQuizzesService quizService,
            IEventsResultsService quizResultService)
        {
            this.userManager = userManager;
            this.quizService = quizService;
            this.quizResultService = quizResultService;
        }

        public async Task<IActionResult> Start(string password)
        {
            var id = await this.quizService.GetQuizIdByPasswordAsync(password);
            var userId = this.userManager.GetUserId(this.User);
            var userHasPermitionToTakeTheQuiz = await this.quizService.HasUserPermition(userId, id);
            var user = await this.userManager.FindByIdAsync(userId);
            var roles = await this.userManager.GetRolesAsync(user);

            if (!userHasPermitionToTakeTheQuiz)
            {
                if (roles.Count > 0)
                {
                    return this.RedirectToAction("Index", "Home", new
                    {
                        password,
                        area = GlobalConstants.Administration,
                        errorText = GlobalConstants.ErrorMessages.PermissionDenied,
                    });
                }
                else
                {
                    return this.RedirectToAction("Index", "Students", new
                    {
                        password,
                        area = string.Empty,
                        errorText = GlobalConstants.ErrorMessages.PermissionDenied,
                    });
                }
            }

            if (roles.Count > 0)
            {
                this.ViewData["Area"] = Constants.AdminArea;
            }

            // this.HttpContext.Session.SetString(Constants.AttemptedQuizId, id);
            var quizModel = await this.quizService.GetQuizByIdAsync<AttemtedQuizViewModel>(id);
            // this.HttpContext.Session.SetInt32(Constants.QuestionsCount, quizModel.Questions.Count);
            // this.HttpContext.Session.SetString(Constants.AttemptedQuizName, quizModel.Name);

            return this.View(quizModel);
        }

        [HttpPost]
        public async Task<IActionResult> Start(PasswordInputViewModel model)
        {
            if (model.Password == null)
            {
                return this.RedirectToAction("Index", "Students", new
                {
                    password = model.Password,
                    area = string.Empty,
                    errorText = GlobalConstants.ErrorMessages.EmptyPasswordField,
                });
            }

            var id = await this.quizService.GetQuizIdByPasswordAsync(model.Password);

            if (id == null)
            {
                return this.RedirectToAction("Index", "Students", new
                {
                    password = model.Password,
                    area = string.Empty,
                    errorText = string.Format(GlobalConstants.ErrorMessages.QuizNotFound, model.Password),
                });
            }

            return this.RedirectToAction("Start", new { password = model.Password });
        }

        [HttpGet]
        public IActionResult Submit(QuizResultViewModel model)
        {
            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(AttemtedQuizViewModel model)
        {
            var userId = this.userManager.GetUserId(this.User);
            var originalQuizModel = await this.quizService.GetQuizByIdAsync<InputQuizViewModel>(model.Id);
            var resultModel = await this.quizResultService.GetResultModel(model.Id, userId, originalQuizModel.Questions, model.Questions);
            resultModel.QuizName = model.Name;

            return this.View(resultModel);
        }
    }
}
