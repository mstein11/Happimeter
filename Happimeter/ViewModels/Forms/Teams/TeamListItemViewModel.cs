using System;
using Happimeter.Core.Database;
namespace Happimeter.ViewModels.Forms.Teams
{
    public class TeamListItemViewModel : BaseViewModel
    {
        private TeamEntry teamObj;

        public TeamListItemViewModel(TeamEntry team)
        {
            Id = team.Id;
            Name = team.Name;
            IsAdmin = team.IsAdmin;
            teamObj = team;
        }

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private bool _idAdmin;
        public bool IsAdmin
        {
            get => _idAdmin;
            set => SetProperty(ref _idAdmin, value);
        }
    }
}
