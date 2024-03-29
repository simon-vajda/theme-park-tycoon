﻿using System;
using System.Collections.Generic;

namespace WeShallNotPass.Model
{
    public class Model
    {
        private MainEntrance mainEntrance;

        #region Properties

        private Item?[,] _gameArea;
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
        public List<Restroom> Restrooms
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

        private List<Plant> _plants;
        public List<Plant> Plants
        {
            get { return _plants; }
            set { _plants = value; }
        }
        private List<Generator> _generators;

        public List<Generator> Generators
        {
            get { return _generators; }
            set { _generators = value; }
        }


        private int _money;
        public int Money
        {
            get { return _money; }
            set
            {
                _money = value;
                MoneyUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private int _campaignTime;

        public int CampaignTime
        {
            get => _campaignTime;
            private set
            {
                _campaignTime = value;
                CampaignUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private int _time;
        private string _gameTime;

        public string GameTime
        {
            get { return _gameTime; }
            set
            {
                _gameTime = value;
                TimePassed?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                _isOpen = value;
                ParkOpenedOrClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Events 

        public event EventHandler<ItemEventArgs> ItemUpdated;
        public event EventHandler<VisitorEventArgs> VisitorUpdated;
        public event EventHandler<VisitorEventArgs> VisitorRemoved;
        public event EventHandler<EventArgs> CampaignUpdated;
        public event EventHandler<EventArgs> TimePassed;
        public event EventHandler<EventArgs> MoneyUpdated;
        //public event EventHandler<EventArgs> NewGameStarted;
        public event EventHandler<ErrorMessageEventArgs> ErrorMessageCalled;
        public event EventHandler<EventArgs> ParkOpenedOrClosed;
        public event EventHandler<ErrorMessageEventArgs> TimerChanged;

        #endregion

        #region Methods

        public Model()
        {
            _isOpen = false;
        }

        public void Tick()
        {
            _time++;

            if (_time % 10 == 0) // a second has passed
            {
                foreach (Visitor v in _visitors) 
                {
                    if (_time % 30 == 0)
                    {
                        v.RestroomNeeds--;
                        v.Satiety--;
                        if (v.Satiety < 20) v.Mood--;
                        if (v.RestroomNeeds < 20) v.Mood--;
                        if (v.Status != VisitorsStatus.AT_ACTIVITY)
                        {
                            v.Mood--;
                            if (v.Status == VisitorsStatus.WAITING_IN_QUEUE)
                            {
                                v.WaitTime++;
                                v.Mood -= (int)Math.Floor(Math.Sqrt(v.WaitTime / 15));
                            }
                        }
                    }
                    if (v.Status == VisitorsStatus.WALKING)
                    {
                        int maxPlantBoost = 0; // plants boost visitor's mood
                        foreach (Plant p in _plants)
                        {
                            if (Math.Abs((v.X / 64) - p.X) <= p.Radius && Math.Abs((v.Y / 64) - p.Y) <= p.Radius && maxPlantBoost < p.MoodBoost)
                                maxPlantBoost = p.MoodBoost;
                        }
                        v.Mood += maxPlantBoost;
                        
                    }
                }

                foreach (Restaurant r in _restaurants)
                {
                    if (!r.IsBuilt)
                    {
                        r.BuildTime--;
                        if (r.BuildTime < 0) r.IsBuilt = true;
                    }
                    if (r.EndTimer <= 0)
                    {
                        if (r.VisitorQueue.Count >= 1)
                        {
                            Visitor[] vis = r.VisitorQueue.ToArray();
                            if (!vis[0].Leaving)
                            {
                                r.EndTimer = r.Duration;
                                vis[0].Status = VisitorsStatus.AT_ACTIVITY;
                                vis[0].Money -= r.FoodPrice;
                                Money += r.FoodPrice;
                            }
                        }
                    }
                    else
                    {
                        r.EndTimer--;
                        if (r.EndTimer <= 0)
                        {
                            Visitor v = r.VisitorQueue.Dequeue();
                            if (!v.Leaving)
                            {
                                v.Status = VisitorsStatus.WAITING;
                                v.Satiety += r.HungerBoost;
                            }
                        }
                    }
                }
                foreach (Game g in _games)
                {
                    if (!g.IsBuilt)
                    {
                        g.BuildTime--;
                        if (g.BuildTime < 0) g.IsBuilt = true;
                    }

                    if (g.EndTimer <= 0)
                    {
                        if (g.VisitorQueue.Count >= g.MinCapacity)
                        {
                            if (g.VisitorQueue.Count >= g.MaxCapacity)
                            {
                                g.ActLoad = g.MaxCapacity;
                            }
                            else
                            {
                                g.ActLoad = g.VisitorQueue.Count;
                            }
                            Visitor[] vis = g.VisitorQueue.ToArray();
                            g.EndTimer = g.Duration;
                            for (int i = 0; i < g.ActLoad; i++)
                            {
                                if (!vis[i].Leaving)
                                {
                                    vis[i].Status = VisitorsStatus.AT_ACTIVITY;
                                    vis[i].Money -= g.TicketPrice;
                                }
                                else g.ActLoad--;
                            }
                            Money += g.TicketPrice * g.ActLoad;
                            g.IsOperating = true;
                        }
                    }
                    else
                    {
                        g.EndTimer--;
                        if (g.EndTimer <= 0)
                        {
                            for (int i = 0; i < g.ActLoad; i++)
                            {
                                Visitor v = g.VisitorQueue.Dequeue();
                                if (!v.Leaving)
                                {
                                v.Status = VisitorsStatus.WAITING;
                                v.Mood += g.MoodBoost;
                                }
                            }
                            g.IsOperating = false;
                        }
                    }
                }
                foreach (Restroom r in _restrooms)
                {
                    if (!r.IsBuilt)
                    {
                        r.BuildTime--;
                        if (r.BuildTime < 0) r.IsBuilt = true;
                    }
                    if (r.EndTimer <= 0)
                    {
                        if (r.VisitorQueue.Count >= 1)
                        {
                            Visitor[] vis = r.VisitorQueue.ToArray();
                            if (!vis[0].Leaving)
                            {
                                vis[0].Status = VisitorsStatus.AT_ACTIVITY;
                                r.EndTimer = r.Duration;   
                            }
                        }
                    }
                    else
                    {
                        r.EndTimer--;
                        if (r.EndTimer <= 0)
                        {
                            Visitor v = r.VisitorQueue.Dequeue();
                            if (!v.Leaving)
                            {
                                v.Status = VisitorsStatus.WAITING;
                                v.RestroomNeeds = 100;
                            }
                        }
                    }
                }
                foreach (Generator g in _generators)
                {
                    if (!g.IsBuilt)
                    {
                        g.BuildTime--;
                        if (g.BuildTime < 0)
                        {
                            g.IsBuilt = true;
                            foreach (Facility f in _restaurants) f.CheckPower(_gameArea);
                            foreach (Facility f in _games) f.CheckPower(_gameArea);
                            foreach (Facility f in _restrooms) f.CheckPower(_gameArea);
                            ItemUpdated?.Invoke(this, new ItemEventArgs(g));
                        }
                    }
                }

                if (CampaignTime > 0)
                {
                    CampaignTime--;
                }

                TimePassed?.Invoke(this, new EventArgs());
            }

            foreach (Visitor v in _visitors)
            {
                v.VisitorTick(Restrooms, Restaurants, Games, mainEntrance, GameArea, GameAreaSize);
                if (_visitors.Contains(v)) VisitorUpdated?.Invoke(this, new VisitorEventArgs(v));
                if (v.Mood == 0 && !v.Leaving)
                {
                    v.NextActivity(_restrooms, _restaurants, _games, mainEntrance, _gameArea, _gameAreaSize);
                }
            }


            if (IsOpen)
            {
                UpdateGameTime();

                if (_time % 20 == 0)
                {
                    RegisterVisitor();
                }
            }
        }

        public void NewGame()
        {
            _gameArea = new Item[GameAreaSize, GameAreaSize];
            _games = new List<Game>();
            _restaurants = new List<Restaurant>();
            _restrooms = new List<Restroom>();
            _visitors = new List<Visitor>();
            _plants = new List<Plant>();
            _generators = new List<Generator>();
            _money = 15000;
            MoneyUpdated?.Invoke(this, EventArgs.Empty);
            CampaignTime = 0;
            _time = 0;
            GameTime = "00:00";
            IsOpen = false;
            TimePassed?.Invoke(this, EventArgs.Empty);

            mainEntrance = new MainEntrance(7, 13, "Jegyiroda", 1, 1, new Uri("/Images/ticket_office.png", UriKind.Relative), 0, 0, 5, 40);
            Build(mainEntrance);
            Build(new Road(6, 13, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 0, 0));
            //TestMapMaker();
        }

        private void TestMapMaker()
        {
            Build(new Road(6, 12, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(6, 11, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(6, 10, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(6, 9, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(6, 8, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(6, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(5, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(4, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(3, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(2, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Generator(6, 6, "gen", 1, 1, new Uri("/Images/generator.png", UriKind.Relative), 100, 0, 10));
            Build(new Game(2, 4, "Hullámvasút", 3, 3, new Uri("/Images/stills/rollercoaster.gif", UriKind.Relative), 2600, 0, 16, 50, 30, GameArea, 2, 100, 400, 30));
            Build(new Road(7, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(8, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(9, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(10, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Road(11, 7, "Út", 1, 1, new Uri("/Images/ground.png", UriKind.Relative), 100, 0));
            Build(new Restaurant(9, 5, "Gyorsétterem", 2, 2, new Uri("/Images/restaurant.png", UriKind.Relative), 1900, 0, 20, 400, 10, GameArea, 20, 10, 50));
        }

        public void OpenPark()
        {
            IsOpen = true;
        }

        public void StartCampaigning()
        {
            if (CampaignTime == 0)
            {
                if (Money < 300)
                {
                    ErrorMessageCalled?.Invoke(this, new ErrorMessageEventArgs("Nincs elég pénzed! A kampányolás 300-ba kerül."));
                    return;
                }

                Money -= 300;
                CampaignTime = 60;
            }
        }

        public void Build(Item item)
        {
            if (!CanBuildAt(item.X, item.Y, item.SizeX, item.SizeY))
            {
                ErrorMessageCalled?.Invoke(this, new ErrorMessageEventArgs("Ezt ide nem tudod letenni!"));
                return;
            }

            if (item.Price > Money)
            {
                ErrorMessageCalled?.Invoke(this, new ErrorMessageEventArgs("Nincs Elég pénzed!"));
                return;
            }

            for (int i = item.X; i < item.X + item.SizeX; i++)
            {
                for (int j = item.Y; j < item.Y + item.SizeY; j++)
                {
                    GameArea[i, j] = item;
                }
            }

            switch (item)
            {
                case Game game:
                    Games.Add(game);
                    game.CheckPower(_gameArea);
                    game.CheckRechaibility(_gameArea);
                    break;
                case Restaurant restaurant:
                    Restaurants.Add(restaurant);
                    restaurant.CheckPower(_gameArea);
                    restaurant.CheckRechaibility(_gameArea);
                    break;
                case Restroom restroom:
                    Restrooms.Add(restroom);
                    restroom.CheckPower(_gameArea);
                    restroom.CheckRechaibility(_gameArea);
                    break;
                case Plant p:
                    Plants.Add(p);
                    break;
                case Generator gen:
                    Generators.Add(gen);
                    break;
                case Road r:
                    foreach (Facility f in _restaurants) f.CheckRechaibility(_gameArea);
                    foreach (Facility f in _games) f.CheckRechaibility(_gameArea);
                    foreach (Facility f in _restrooms) f.CheckRechaibility(_gameArea);
                    break;
            }

            ItemUpdated?.Invoke(this, new ItemEventArgs(item));
            Money -= item.Price;
        }

        private bool CanBuildAt(int x, int y, int sizeX, int sizeY)
        {
            int xEnd = x + sizeX;
            int yEnd = y + sizeY;

            if (xEnd > GameAreaSize || yEnd > GameAreaSize) return false;

            for (int i = x; i < xEnd; i++)
            {
                for (int j = y; j < yEnd; j++)
                {
                    if (GameArea[i, j] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void RegisterVisitor()
        {
            Random random = new Random();

            int maxCapacity = 0;
            foreach (Game g in Games) maxCapacity += g.MaxCapacity;
            foreach (Restaurant r in Restaurants) maxCapacity += r.MaxCapacity;
            foreach (Restroom r in Restrooms) maxCapacity += r.MaxCapacity;

            if (maxCapacity == 0) return;

            double fullness = Visitors.Count / (double)maxCapacity;

            if (fullness > 1f) return;

            double d1 = mainEntrance.TicketPrice / 100f;
            d1 = d1 * (-1) + 1;

            double d2 = fullness * (-1) + 1;

            double willingness = d1 * d2 * 100;

            if (CampaignTime > 0) willingness *= 1.5f;

            if (random.Next(100) > willingness) return;

            string img;
            int imgNumber = random.Next(4);

            if (imgNumber == 0) img = "/Images/characters/red.png";
            else if (imgNumber == 1) img = "/Images/characters/yellow.png";
            else if (imgNumber == 2) img = "/Images/characters/green.png";
            else img = "/Images/characters/cyan.png";

            int visitorMoney = random.Next(20, 50) * 10;

            // There is a 40% chance, that the visitor has a coupon while campaigning
            bool hasCoupon = CampaignTime > 0 && random.Next(100) < 60;

            if (!hasCoupon)
            {
                Money += mainEntrance.TicketPrice;
                visitorMoney -= mainEntrance.TicketPrice;
            }


            Visitor v = new Visitor(6 * 64, 13 * 64, visitorMoney, random.Next(20, 100), random.Next(20, 100), random.Next(70, 100), new Uri(img, UriKind.Relative));
            Visitors.Add(v);
            VisitorUpdated?.Invoke(this, new VisitorEventArgs(v));
            v.VisitorArrived += VisitorArrived;
        }

        private void VisitorArrived(object sender, VisitorEventArgs e)
        {
            Visitor visitor = (Visitor)sender;
            visitor.WaitTime = 0;
            if (visitor.Destination is MainEntrance)
            {
                RemoveVisitor(visitor);
            }
            else
            {
                ((Facility)visitor.Destination).VisitorQueue.Enqueue(visitor);
                ItemUpdated?.Invoke(this, new ItemEventArgs(visitor.Destination));
            }
        }

        private void RemoveVisitor(Visitor v)
        {
            List<Visitor> list = new List<Visitor>();
            foreach (Visitor vi in _visitors)
            {
                if (vi != v) list.Add(vi);
            }
            _visitors = list;
            VisitorRemoved?.Invoke(this, new VisitorEventArgs(v));
        }

        public void ChangeTimer(string sp)
        {
            TimerChanged.Invoke(this, new ErrorMessageEventArgs(sp));
        }

        private void UpdateGameTime()
        {
            int hour = (_time / 60) % 24;
            int minute = _time % 60;
            GameTime = String.Format("{0:D2}:{1:D2}", hour, minute);
        }
        #endregion
    }
}
