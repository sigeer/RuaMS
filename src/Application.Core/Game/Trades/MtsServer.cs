using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Game.Trades
{
    public class MtsServer: ITradeServer
    {
        int _currentTab = 1;
        int _currentType;
        int _currentPage;
        int _currentSearchType;
        string? _currentSearch;

        public int CurrentTab => _currentTab;
        public int CurrentType => _currentType;
        public int CurrentPage => _currentPage;
        public int CurrentSearchType => _currentSearchType;
        public string? CurrentSearch => _currentSearch;

        public void ChangeTab(int tab) => _currentTab = tab;
        public void ChangeType(int type) => _currentType = type;
        public void ChangePage(int page) => _currentPage = page;
        public void ChangeSearch(int searchType, string? search)
        {
            _currentSearchType = searchType;
            _currentSearch = search;
        }
    }
}
