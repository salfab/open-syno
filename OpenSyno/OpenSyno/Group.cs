namespace OpenSyno
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;

    using OpenSyno.ViewModels;

    public class Group<T> : IEnumerable<T>
    {
        public Group(object headerContent, IEnumerable<T> items)
        {
            this.HeaderContent = headerContent;
            this.Items = new List<T>(items);
        }

        public override bool Equals(object obj)
        {
            Group<T> that = obj as Group<T>;
            return (that != null) && (HeaderContent.Equals(that.HeaderContent));
        }

        public object HeaderContent { get; set; }
        public IList<T> Items { get; set; }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        #endregion

        public void AddRange(IGrouping<char, T> group)
        {
            foreach (var artist in group)
            {
                Items.Add(artist);
            }
        }
    }

    public class AlbumGroupViewModel<T> : Group<T> where T : TrackViewModel
    {
        public AlbumGroupViewModel(object headerContent, IEnumerable<T> items) 
            : base(headerContent, items)
        {
            SelectAllAlbumTracksCommand = new DelegateCommand(() =>
                {
                    bool newSelectedValue = !this.Items.First().IsSelected;
                    foreach (T item in this.Items)
                    {
                        item.IsSelected = newSelectedValue;
                    }
                });
        }

        public ICommand SelectAllAlbumTracksCommand { get; set; }
    }
}
