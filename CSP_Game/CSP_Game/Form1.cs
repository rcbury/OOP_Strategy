﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MyPhotoshop.Data;
using MyPhotoshop;

namespace CSP_Game
{
    public partial class Form1 : Form
    {
        Photo map;
        int playerIndex;
        List<Player> players;
        Player currentPlayer;
        bool bIsBuilding = false; // Check if user wants to build smth
        Unit selectedUnit;        // Contains selected unit;
        Player p = new Player("UnitInit", default);

        public void InitializeMap()
        {
            map = new Photo(50, 50);
            pictureBox1.Height = map.height * map.pixelHeight;
            pictureBox1.Width = map.width * map.pixelWidth;
            for (int x = 0; x < map.width; x++)
                for (int y = 0; y < map.height; y++)
                {
                    map[x, y] = new Pixel(1, 1, 1);
                }
        }

        public void InitializePlayers()
        {
            players = new List<Player>();
            players.Add(new Player("Andrew", Color.Green));
            players.Add(new Player("Roman", Color.Crimson));
            playerIndex = 0;
            currentPlayer = players[playerIndex];
            Text = currentPlayer.Name;
        }

        public Form1()
        {
            InitializeComponent();
            InitializeMap();
            InitializePlayers();
            pictureBox1.Image = Convertors.Photo2Bitmap(map);
            Type[] masterySelector = new Type[]
            {
                typeof(Tank),
                typeof(RifleMan),
                typeof(Tower),
                typeof(MiningCamp)
            };
            comboBox1.DataSource = masterySelector;
            comboBox1.DisplayMember = "Name";
            PlayerTurn.OnTurnStart(currentPlayer);
            label2.Text = currentPlayer.Treasure.ToString();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (players.Where(player => player.IsAlive).Count() != 1)
            {
                playerIndex++;
                if (playerIndex > players.Count - 1)
                    playerIndex = 0;
                currentPlayer = players[playerIndex];
                Text = currentPlayer.Name;
                PlayerTurn.OnTurnStart(currentPlayer);
                label2.Text = currentPlayer.Treasure.ToString();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bIsBuilding = true;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var x = (int)Math.Floor((double)e.X / map.pixelWidth); // X relatively form
            var y = (int)Math.Floor((double)e.Y / map.pixelHeight);// Y relatively form
            MoveOrSelectUnit(x, y);
            TryBuild(x, y);
            pictureBox1.Image = Convertors.Photo2Bitmap(map);
        }

        private void TryBuild(int x, int y)
        {
            if (bIsBuilding)
            {
                Type selectedObject = (Type)comboBox1.SelectedValue;
                AnyObject objectToBuild = (AnyObject)selectedObject
                    .GetConstructor(new Type[] { typeof(Player), typeof(Tuple<int,int>) })
                    .Invoke(new object[] { currentPlayer, new Tuple<int,int>(x,y) });
                PlayerTurn.Build(currentPlayer, objectToBuild, new Tuple<int, int>(x, y));
                for (int i = x - objectToBuild.Border; i <= x + objectToBuild.Border; i++)
                {
                    for (int j = y - objectToBuild.Border; j <= y + objectToBuild.Border; j++)
                    {
                        map[i, j] = new Pixel((double)currentPlayer.Color.R / 255,
                                              (double)currentPlayer.Color.G / 255,
                                              (double)currentPlayer.Color.B / 255);
                    }
                }
                bIsBuilding = false;
            }
        }

        private void MoveOrSelectUnit(int x, int y)
        {
            if (selectedUnit != null)
            {
                Drawer.DrawObject(Color.FromArgb(255, 255, 255), selectedUnit.Border,
                    selectedUnit.Position.Item1, selectedUnit.Position.Item2, map);
                Drawer.DrawObject(currentPlayer.Color, selectedUnit.Border, x, y, map);
                PlayerTurn.MoveSelectedUnit(currentPlayer, selectedUnit, new Tuple<int, int>(x, y));
                selectedUnit = null;
            }
            else
            {
                selectedUnit = PlayerTurn.ReturnSelectedUnit(currentPlayer, new Tuple<int, int>(x, y));
                if (selectedUnit != null)
                    progressBar1.Value = (int)(selectedUnit.HP / selectedUnit.FullHP) * 100;
                else
                    progressBar1.Value = 0;
            }
        }
    }
}
