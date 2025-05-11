namespace PrivateLMS.ViewModels
{
    public class UserPreferenceViewModel
    {
        public bool PreferencesSetup { get; set; }

        public List<int> SelectedCategories { get; set; } = new();
        public List<int> SelectedAuthors { get; set; } = new();
        public List<int> SelectedLanguages { get; set; } = new();
    }
}
