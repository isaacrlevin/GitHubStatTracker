using GitHubStatTracker.Core.Models;
using GitHubStatTracker.Web.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubStatTracker.Web.Extensions
{
    public static class OctokitExtensions
    {
        public static RepoViewModel ConvertToViewModel(this Repository context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new RepoViewModel
            {
                Name = context.Name,
                Description = context.Description,
                ForksCount = context.ForksCount,
                Id = (int)context.Id,
                StarCount = context.StargazersCount,
                UpdatedAt = context.UpdatedAt.DateTime,
                RepoUrl = context.HtmlUrl
            };
        }

        public static List<SummaryViewModel> ConvertToViewModel(this RepositoryTrafficViewSummary context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var list = new List<SummaryViewModel>();

            foreach (var item in context.Views.ToList())
            {
                list.Add(new SummaryViewModel
                {
                    Date = item.Timestamp.DateTime,
                    Count = item.Count,
                    Uniques = item.Uniques
                });
            }

            return list;
        }

        public static List<SummaryViewModel> ConvertToViewModel(this RepositoryTrafficCloneSummary context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var list = new List<SummaryViewModel>();

            foreach (var item in context.Clones.ToList())
            {
                list.Add(new SummaryViewModel
                {
                    Date = item.Timestamp.DateTime,
                    Count = item.Count,
                    Uniques = item.Uniques
                });
            }

            return list;
        }

        public static TrafficViewModel ConvertToViewModel(this List<RepoStatEntity> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var model = new TrafficViewModel();


            if (context.Count > 0)
            {
                model.Repo = new RepoViewModel
                {
                    Name = context[0].RepoName,
                    Id = context[0].RepoId
                };

                model.ViewSummary = new List<SummaryViewModel>();
                model.CloneSummary = new List<SummaryViewModel>();
                model.StarsSummary = new List<SummaryViewModel>();
                model.ForksSummary = new List<SummaryViewModel>();

                foreach (var item in context)
                {
                    model.ViewSummary.Add(new SummaryViewModel
                    {
                        Count = item.Views,
                        Uniques = item.UniqueUsers,
                        Date = DateTime.Parse(item.Date)
                    });

                    model.CloneSummary.Add(new SummaryViewModel
                    {
                        Count = item.Clones,
                        Uniques = item.UniqueClones,
                        Date = DateTime.Parse(item.Date)
                    });

                    model.ForksSummary.Add(new SummaryViewModel
                    {
                        Count = item.ForksCount,
                        Date = DateTime.Parse(item.Date)
                    });

                    model.StarsSummary.Add(new SummaryViewModel
                    {
                        Count = item.StarCount,
                        Date = DateTime.Parse(item.Date)
                    });

                }
            }
            return model;
        }
    }
}
