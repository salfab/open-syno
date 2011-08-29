using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Ninject;

namespace OpenSyno.ViewModels
{
    using System.Collections;
    using System.Linq;

    using Microsoft.Practices.Prism.Events;

    using System;
    using System.Collections.Generic;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public class SearchResultsViewModel : ViewModelBase, ISearchResultsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultsViewModel"/> class.
        /// </summary>
        /// <param name="pageSwitchingService">The page switching service.</param>
        public SearchResultsViewModel(IPageSwitchingService pageSwitchingService, ISearchResultItemViewModelFactory searchResultItemViewModelFactory)
        {
            if (pageSwitchingService == null) throw new ArgumentNullException("pageSwitchingService");

            // register for search results updates
            _eventAggregator = IoC.Container.Get<IEventAggregator>();
            _pageSwitchingService = pageSwitchingService;
            _searchResultItemViewModelFactory = searchResultItemViewModelFactory;

            FilterResultsCommand = new DelegateCommand<string>(OnFilterResults);
            // everytime the searchResults changes, we'll react to that change.
            _eventAggregator.GetEvent<CompositePresentationEvent<SearchResultsRetrievedAggregatedEvent>>().Subscribe(SearchResultsUpdated, true);
        }

        private void OnFilterResults(string filterExpression)
        {
            SearchResults = _unfilteredSearchResults.Where(o => o.Title.ToLowerInvariant().Contains(filterExpression.ToLowerInvariant())).Select(o => _searchResultItemViewModelFactory.Create(o, this._eventAggregator, this._pageSwitchingService));
        }

        private void SearchResultsUpdated(SearchResultsRetrievedAggregatedEvent payload)
        {
            _unfilteredSearchResults = payload.Results;
            SearchResults = from result in payload.Results select _searchResultItemViewModelFactory.Create(result, this._eventAggregator, this._pageSwitchingService);
        }

        private IEnumerable<SearchResultItemViewModel> _searchResults;
        private readonly IEventAggregator _eventAggregator;
        private readonly IPageSwitchingService _pageSwitchingService;
        private IEnumerable<ISynoItem> _unfilteredSearchResults;
        private const string ArtistsPropertyName = "Artists";
        private const string SearchResultsPropertyName = "SearchResults";

        public IEnumerable<SearchResultItemViewModel> SearchResults
        {
            get { return _searchResults; }
            set
            {
                _searchResults = value;
                OnPropertyChanged(SearchResultsPropertyName);
                OnPropertyChanged(ArtistsPropertyName);
            }
        }

        public ICommand FilterResultsCommand { get; set; }

        private string _filterExpression;

        private ISearchResultItemViewModelFactory _searchResultItemViewModelFactory;

        private const string FilterExpressionPropertyName = "FilterExpression";

        public string FilterExpression
        {
            get { return _filterExpression; }
            set
            {
                _filterExpression = value;
                OnPropertyChanged(FilterExpressionPropertyName);
            }
        }

        public IEnumerable Artists
        {
            get
            {
                if (SearchResults == null)
                {
                    return new List<Group<SearchResultItemViewModel>>();
                }

                // var artists = new List<Group<SearchResultItemViewModel>>(from artist in SearchResults.Where(o => o.ItemInfo.ItemPid == "musiclib_music_aa") group artist by artist.ItemInfo.Title.FirstOrDefault() into c select new Group<SearchResultItemViewModel>(char.IsLetter(c.Key) ? c.Key.ToString().ToLower() : "#", c));
                var groups = from artist in SearchResults.Where(o => o.ItemInfo.ItemPid == "musiclib_music_aa") group artist by artist.ItemInfo.Title.FirstOrDefault();
                var artists = new List<Group<SearchResultItemViewModel>>();
                foreach (var group in groups)
                {
                    char firstChar = group.Key.ToString().FirstOrDefault();
                    string groupName = char.IsLetter(firstChar) ? firstChar.ToString().ToLower() : "#";
                    if (artists.Any(o => o.Title == groupName))
                    {
                        // Add artist to group
                        artists.First(o => o.Title == groupName).AddRange(group);
                    }
                    else
                    {
                        // Add a new group
                        artists.Add(new Group<SearchResultItemViewModel>(groupName, group));
                    }
                }

                AddEmptyGroups(artists);

                //var tempArtists = from a in SearchResults.Where(o => o.Class == "Artist") select a;

                //new { StartsWith = a.Artist.FirstOrDefault(), Artist = a.Artist, Class = a.Class, a.ItemID, a.ItemPid };
                //var artists = tempArtists.GroupBy(o => o.);
                //from city in source 
                //    group city by city.Country into c   
                //        orderby c.Key     
                //            select new Group<City>(c.Key, c);
                IOrderedEnumerable<Group<SearchResultItemViewModel>> orderedEnumerable = artists.OrderBy(o => o.Title);
                return orderedEnumerable;
            }
        }

        private void AddEmptyGroups<T>(List<Group<T>> items)
        {
            for (int i = 97; i < 123; i++)
            {
                var titleChar = (char)i;
                if (!items.Any(o => o.Title.Equals(titleChar.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    items.Add(new Group<T>(titleChar.ToString(), new List<T>()));
                }
            }
        }
    }

    public class SearchResultItemViewModelFactory : ISearchResultItemViewModelFactory
    {
        private IUrlParameterToObjectsPlateHeater _urlParameterToObjectsPlateHeater;

        public SearchResultItemViewModelFactory(IUrlParameterToObjectsPlateHeater urlParameterToObjectsPlateHeater)
        {
            this._urlParameterToObjectsPlateHeater = urlParameterToObjectsPlateHeater;
        }

        // Eventaggregator needs to be moved to .ctor
        public SearchResultItemViewModel Create(ISynoItem result, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService)
        {
            return new SearchResultItemViewModel(result, eventAggregator, pageSwitchingService, _urlParameterToObjectsPlateHeater);
        }
    }

    public interface ISearchResultItemViewModelFactory
    {
        SearchResultItemViewModel Create(ISynoItem result, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService);
    }
}