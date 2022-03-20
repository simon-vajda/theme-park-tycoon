﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WeShallNotPass.Model
{
    public class Model
    {
        #region Properties

        private Item[,] _gameArea;
        public Item[,] GameArea
        {
            get { return _gameArea; }
            set { _gameArea = value; }
        }

        private int _gameAreaSize;
        public int GameAreaSize
        {
            get { return _gameAreaSize; }
            set { _gameAreaSize = value; }
        }


        private List<Game> _games;
        public List<Game> Games
        {
            get { return _games; }
            set { _games = value; }
        }

        private List<Restaurant> _restaurants;
        public List<Restaurant> Restaurants
        {
            get { return _restaurants; }
            set { _restaurants = value; }
        }

        private List<Restroom> _restrooms;
        public List<Restroom> Restroom
        {
            get { return _restrooms; }
            set { _restrooms = value; }
        }

        private List<Visitor> _visitors;   
        public List<Visitor> Visitors
        {
            get { return _visitors; }
            set { _visitors = value; }
        }

        private int _money;
        public int Money
        {
            get { return _money; }
            set { _money = value; }
        }

        private bool _isCampaigning;

        public bool IsCampaigning
        {
            get { return _isCampaigning; }
            set { _isCampaigning = value; }
        }
        private int _campaignEnd;

        public int CampaignEnd
        {
            get { return _campaignEnd; }
            set { _campaignEnd = value; }
        }

        private int _time;

        public int Time
        {
            get { return _time; }
            set { _time = value; }
        }

        #endregion

        #region Fields

        private DispatcherTimer _timer;
        #endregion

        #region Events 

        public event EventHandler<EventArgs> ItemUpdated;
        public event EventHandler<EventArgs> VisitorsUpdated;
        public event EventHandler<EventArgs> CampaignUpdated;
        public event EventHandler<EventArgs> TimePassed;
        public event EventHandler<EventArgs> MoneyUpdated;

        #endregion

        #region Methods

        public Model()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += _timer_Tick;

            /*_gameAreaSize = 16;
            _gameArea = new Item[GameAreaSize,GameAreaSize];
            _games = new List<Game>();
            _restaurants = new List<Restaurant>();
            _restrooms = new List<Restroom>();
            _money = 10000;
            _isCampaigning = false;
            _time = 0;*/
            NewGame();

        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            Time += 1;
        }

        public void NewGame()
        {
            _gameAreaSize = 14;
            _gameArea = new Item[GameAreaSize, GameAreaSize];
            _games = new List<Game>();
            _restaurants = new List<Restaurant>();
            _restrooms = new List<Restroom>();
            _money = 10000;
            _isCampaigning = false;
            _time = 0;

            _timer.Start();


        }
        public void OpenPark()
        {
            throw new NotImplementedException();
        }

        public void ClosePark()
        {
            throw new NotImplementedException();
        }

        public void Build(Item item)
        {
            throw new NotImplementedException();
        }

        public void Demolish(Item item)
        {
            throw new NotImplementedException();
        }

        private bool CanBuildAt(int x, int y, int sizeX, int sizeY)
        {
            throw new NotImplementedException();
        }

        private void registerVisitor()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
