﻿using System;
using System.Drawing;
using PKHeX.Core;
using System.Windows.Forms;

namespace FeebasLocatorPlugin
{
    public partial class FeebasLocatorForm : Form
    {
        private readonly SaveFile sav;
        private uint Seed;
        private readonly int SeedOffset;
        private readonly Panel[] Marker = new Panel[6];

        public FeebasLocatorForm(SaveFile sav)
        {
            this.sav = sav;
            InitializeComponent();

            for (int i = 0; i < Marker.Length; i++)
                Marker[i] = new Panel();

            switch (sav.Version)
            {
                case GameVersion.S:
                case GameVersion.R:
                case GameVersion.RS:
                    SeedOffset = 0x2DD4 + 2; // DewfordTrend
                    break;
                case GameVersion.E:
                    SeedOffset = 0x2E64 + 6; // DewfordTrend
                    break;
                case GameVersion.D:
                case GameVersion.P:
                case GameVersion.DP:
                    SeedOffset = 0x53C8;
                    break;
                case GameVersion.Pt:
                    SeedOffset = 0x5664;
                    break;
            }

            if (sav is SAV3 s3)
            {
                SetupGen3Form();
                Seed = BitConverter.ToUInt16(s3.Large, SeedOffset);
            }
            else if (sav is SAV4 s4)
            {
                SetupGen4Form();
                Seed = BitConverter.ToUInt32(s4.General, SeedOffset);
            }

            FeebasSeedBox.Text = Seed.ToString("X");
        }

        private void SetupGen3Form()
        {
            ClientSize = new Size(685, 359);
            MaximumSize = new Size(701, 1000);
            MinimumSize = new Size(701, 223);

            FeebasLocatorPanel.Size = new Size(660, 290);

            TilePanel.MaximumSize = new Size(641, 1553);
            TilePanel.MinimumSize = new Size(641, 1553);
            TilePanel.Size = new Size(641, 1553);
            TilePanel.BackgroundImage = Properties.Resources.route119;

            LocationLabel.Text = "Route 119";

            FeebasSeedBox.Size = new Size(52, 20);
            FeebasSeedBox.MaxLength = 4;
        }

        private void SetupGen4Form()
        {
            ClientSize = new Size(459, 359);
            MaximumSize = new Size(485, 495);
            MinimumSize = new Size(485, 223);

            FeebasLocatorPanel.Size = new Size(444, 290);

            TilePanel.MaximumSize = new Size(425, 386);
            TilePanel.MinimumSize = new Size(425, 386);
            TilePanel.Size = new Size(425, 386);
            TilePanel.BackgroundImage = Properties.Resources.mtcoronet;

            LocationLabel.Text = "Mt. Coronet";

            FeebasSeedBox.Size = new Size(82, 20);
            FeebasSeedBox.MaxLength = 8;
        }

        private void MarkTiles(ushort[] tiles)
        {
            for (int i = 0; i < tiles.Length && i < 6; i++)
            {
                int x = 0;
                int y = 0;
                int width = 0;
                int height = 0;

                if (sav is SAV3)
                {
                    x = TileCoordinatesGen3[tiles[i] - 4, 0];
                    y = TileCoordinatesGen3[tiles[i] - 4, 1];
                    width = 15;
                    height = 15;

                    Marker[i].Visible = Feebas3.IsAccessible(tiles[i]);

                    // for some reason all the tiles under the bridge in the north of Route 119 are considered to be tile 132 ...
                    if (Feebas3.IsUnderBridge(tiles[i]))
                        MarkGen3UnderBridgeTiles();
                }
                else if (sav is SAV4)
                {
                    x = TileCoordinatesGen4[tiles[i], 0];
                    y = TileCoordinatesGen4[tiles[i], 1];
                    width = TileCoordinatesGen4[tiles[i], 2];
                    height = TileCoordinatesGen4[tiles[i], 3];

                    Marker[i].Visible = Feebas4.IsAccessible(tiles[i]);
                }
                
                Marker[i].BackColor = Color.Transparent;
                Marker[i].BackgroundImage = Properties.Resources.marker;
                Marker[i].Location = new Point(x, y);
                Marker[i].Name = "Marker" + i;
                Marker[i].Size = new Size(width, height);

                TilePanel.Controls.Add(Marker[i]);
            }
        }

        private void MarkGen3UnderBridgeTiles()
        {        
            int[,] TileCoordinates =
            {
                {257, 257}, {273, 257}, {289, 257}, {305, 257}, {321, 257},
                {257, 273}, {273, 273}, {289, 273}, {305, 273}, {321, 273}
            };

            Panel[] Marker = new Panel[10];

            for (int i = 0; i < 10; i++)
            {
                Marker[i] = new Panel
                {
                    BackColor = Color.Transparent,
                    BackgroundImage = Properties.Resources.marker,
                    Location = new Point(TileCoordinates[i, 0], TileCoordinates[i, 1]),
                    Name = "MarkerUnderBridge" + i,
                    Size = new Size(15, 15),
                    Visible = true
                };

                TilePanel.Controls.Add(Marker[i]);
            }
        }

        private void FeebasSeedBox_TextChanged(object sender, EventArgs e)
        {
            Seed = Util.GetHexValue(FeebasSeedBox.Text);

            if (sav is SAV3)
                MarkTiles(Feebas3.GetTiles(Seed));
            else if (sav is SAV4)
                MarkTiles(Feebas4.GetTiles(Seed));
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (sav is SAV3 s3)
            {
                BitConverter.GetBytes(Util.GetHexValue(FeebasSeedBox.Text)).CopyTo(s3.Large, SeedOffset);
            }
            else if (sav is SAV4 s4)
            {
                BitConverter.GetBytes(Util.GetHexValue(FeebasSeedBox.Text)).CopyTo(s4.General, SeedOffset);
            }
            Close();
        }

        // {x, y}, width & height always 15
        private readonly int[,] TileCoordinatesGen3 =
        {
            {289, 17}, {289, 33}, {305, 33}, {257, 49}, {273, 49}, {289, 49}, {305, 49}, {273, 65}, {289, 65}, {305, 65},
            {273, 81}, {289, 81}, {305, 81}, {273, 97}, {289, 97}, {305, 97}, {273, 113}, {289, 113}, {305, 113}, {273, 193},
            {289, 193}, {305, 193}, {273, 209}, {289, 209}, {305, 209}, {257, 225}, {273, 225}, {289, 225}, {305, 225}, {321, 225},
            {257, 241}, {273, 241}, {289, 241}, {305, 241}, {321, 241}, {257, 289}, {273, 289}, {289, 289}, {305, 289}, {321, 289},
            {257, 305}, {273, 305}, {289, 305}, {305, 305}, {321, 305}, {257, 321}, {273, 321}, {289, 321}, {305, 321}, {321, 321},
            {257, 337}, {273, 337}, {289, 337}, {305, 337}, {321, 337}, {337, 337}, {353, 337}, {257, 353}, {273, 353}, {289, 353},
            {305, 353}, {321, 353}, {337, 353}, {353, 353}, {257, 369}, {273, 369}, {289, 369}, {305, 369}, {321, 369}, {337, 369},
            {353, 369}, {257, 385}, {273, 385}, {289, 385}, {305, 385}, {321, 385}, {337, 385}, {353, 385}, {257, 401}, {273, 401},
            {289, 401}, {305, 401}, {321, 401}, {337, 401}, {353, 401}, {369, 401}, {257, 417}, {273, 417}, {289, 417}, {305, 417},
            {321, 417}, {337, 417}, {353, 417}, {369, 417}, {385, 417}, {401, 417}, {417, 417}, {433, 417}, {449, 417}, {465, 417},
            {481, 417}, {305, 433}, {321, 433}, {337, 433}, {353, 433}, {369, 433}, {385, 433}, {401, 433}, {417, 433}, {433, 433},
            {449, 433}, {465, 433}, {481, 433}, {497, 433}, {513, 433}, {321, 449}, {337, 449}, {353, 449}, {369, 449}, {385, 449},
            {401, 449}, {417, 449}, {433, 449}, {449, 449}, {465, 449}, {481, 449}, {497, 449}, {513, 449}, {369, 465}, {385, 465},
            {401, 465}, {417, 465}, {433, 465}, {449, 465}, {465, 465}, {481, 465}, {497, 465}, {513, 465}, {529, 465}, {545, 465},
            {433, 481}, {449, 481}, {465, 481}, {481, 481}, {497, 481}, {513, 481}, {529, 481}, {545, 481}, {481, 497}, {497, 497},
            {513, 497}, {529, 497}, {545, 497}, {497, 513}, {513, 513}, {529, 513}, {545, 513}, {561, 513}, {497, 529}, {513, 529},
            {529, 529}, {497, 545}, {513, 545}, {529, 545}, {497, 561}, {513, 561}, {529, 561}, {545, 561}, {561, 561}, {497, 577},
            {513, 577}, {529, 577}, {545, 577}, {561, 577}, {529, 593}, {545, 593}, {561, 593}, {529, 609}, {545, 609}, {561, 609},
            {497, 625}, {513, 625}, {529, 625}, {545, 625}, {561, 625}, {497, 641}, {513, 641}, {529, 641}, {545, 641}, {561, 641},
            {481, 657}, {497, 657}, {513, 657}, {529, 657}, {545, 657}, {561, 657}, {433, 673}, {449, 673}, {465, 673}, {481, 673},
            {497, 673}, {513, 673}, {529, 673}, {545, 673}, {417, 689}, {433, 689}, {449, 689}, {465, 689}, {481, 689}, {497, 689},
            {513, 689}, {529, 689}, {417, 705}, {433, 705}, {449, 705}, {465, 705}, {481, 705}, {497, 705}, {417, 721}, {433, 721},
            {449, 721}, {465, 721}, {481, 721}, {497, 721}, {417, 737}, {433, 737}, {449, 737}, {465, 737}, {481, 737}, {497, 737},
            {513, 737}, {385, 753}, {401, 753}, {417, 753}, {433, 753}, {449, 753}, {465, 753}, {385, 769}, {401, 769}, {417, 769},
            {433, 769}, {449, 769}, {353, 785}, {369, 785}, {385, 785}, {401, 785}, {417, 785}, {433, 785}, {449, 785}, {353, 801},
            {369, 801}, {385, 801}, {401, 801}, {417, 801}, {433, 801}, {449, 801}, {353, 817}, {369, 817}, {385, 817}, {401, 817},
            {417, 817}, {321, 833}, {337, 833}, {353, 833}, {369, 833}, {385, 833}, {401, 833}, {417, 833}, {321, 849}, {337, 849},
            {353, 849}, {369, 849}, {385, 849}, {401, 849}, {321, 865}, {337, 865}, {353, 865}, {369, 865}, {385, 865}, {401, 865},
            {321, 881}, {337, 881}, {353, 881}, {369, 881}, {385, 881}, {401, 881}, {321, 897}, {337, 897}, {353, 897}, {369, 897},
            {337, 913}, {337, 929}, {337, 1057}, {353, 1057}, {369, 1057}, {225, 1297}, {225, 1313}, {225, 1329}, {241, 1329}, {257, 1329},
            {305, 1329}, {321, 1329}, {369, 1329}, {385, 1329}, {225, 1345}, {241, 1345}, {257, 1345}, {273, 1345}, {289, 1345}, {305, 1345},
            {321, 1345}, {337, 1345}, {353, 1345}, {369, 1345}, {385, 1345}, {225, 1361}, {241, 1361}, {257, 1361}, {273, 1361}, {289, 1361},
            {305, 1361}, {321, 1361}, {337, 1361}, {353, 1361}, {369, 1361}, {385, 1361}, {225, 1377}, {241, 1377}, {257, 1377}, {273, 1377},
            {289, 1377}, {305, 1377}, {321, 1377}, {337, 1377}, {353, 1377}, {369, 1377}, {385, 1377}, {225, 1393}, {241, 1393}, {257, 1393},
            {273, 1393}, {289, 1393}, {305, 1393}, {321, 1393}, {337, 1393}, {353, 1393}, {369, 1393}, {385, 1393}, {401, 1393}, {225, 1409},
            {241, 1409}, {257, 1409}, {273, 1409}, {289, 1409}, {305, 1409}, {321, 1409}, {337, 1409}, {353, 1409}, {369, 1409}, {385, 1409},
            {401, 1409}, {145, 1425}, {161, 1425}, {177, 1425}, {193, 1425}, {209, 1425}, {225, 1425}, {241, 1425}, {257, 1425}, {273, 1425},
            {289, 1425}, {305, 1425}, {321, 1425}, {369, 1425}, {385, 1425}, {401, 1425}, {113, 1441}, {129, 1441}, {145, 1441}, {161, 1441},
            {177, 1441}, {193, 1441}, {209, 1441}, {225, 1441}, {241, 1441}, {257, 1441}, {273, 1441}, {289, 1441}, {305, 1441}, {321, 1441},
            {369, 1441}, {385, 1441}, {401, 1441}, {145, 1457}, {161, 1457}, {177, 1457}, {193, 1457}, {209, 1457}, {225, 1457}, {241, 1457},
            {257, 1457}, {273, 1457}, {289, 1457}, {305, 1457}, {321, 1457}, {337, 1457}, {353, 1457}, {369, 1457}, {385, 1457}, {401, 1457},
            {145, 1473}, {161, 1473}, {177, 1473}, {193, 1473}, {209, 1473}, {225, 1473}, {241, 1473}, {257, 1473}, {273, 1473}, {289, 1473},
            {305, 1473}, {321, 1473}, {337, 1473}, {353, 1473}, {369, 1473}, {129, 1489}, {145, 1489}, {161, 1489}, {177, 1489}, {193, 1489},
            {209, 1489}, {225, 1489}, {241, 1489}, {257, 1489}, {273, 1489}, {289, 1489}, {305, 1489}, {129, 1505}, {145, 1505}, {161, 1505},
            {177, 1505}, {193, 1505}, {113, 1521}, {129, 1521}
        };

        // {x, y, width, height}
        private readonly int[,] TileCoordinatesGen4 =
        {
            {119, 35, 14, 9}, {134, 35, 13, 9}, {148, 35, 12, 9}, {161, 35, 11, 9}, {173, 35, 12, 9}, {186, 35, 12, 9}, {199, 35, 13, 9}, {213, 35, 14, 9}, {228, 35, 12, 9}, {241, 35, 11, 9},
            {253, 35, 13, 9}, {267, 35, 13, 9}, {281, 35, 11, 9}, {293, 35, 11, 9}, {305, 35, 12, 9}, {318, 35, 14, 9}, {333, 35, 13, 9}, {347, 35, 13, 9}, {119, 45, 14, 9}, {134, 45, 13, 9},
            {148, 45, 12, 9}, {161, 45, 11, 9}, {173, 45, 12, 9}, {186, 45, 12, 9}, {199, 45, 13, 9}, {213, 45, 14, 9}, {228, 45, 12, 9}, {241, 45, 11, 9}, {253, 45, 13, 9}, {267, 45, 13, 9},
            {281, 45, 11, 9}, {293, 45, 11, 9}, {305, 45, 12, 9}, {318, 45, 14, 9}, {333, 45, 13, 9}, {347, 45, 13, 9}, {119, 55, 14, 8}, {134, 55, 13, 8}, {148, 55, 12, 8}, {161, 55, 11, 8},
            {173, 55, 12, 8}, {186, 55, 12, 8}, {199, 55, 13, 8}, {213, 55, 14, 8}, {228, 55, 12, 8}, {241, 55, 11, 8}, {253, 55, 13, 8}, {267, 55, 13, 8}, {281, 55, 11, 8}, {293, 55, 11, 8},
            {305, 55, 12, 8}, {333, 55, 13, 8}, {347, 55, 13, 8}, {119, 64, 14, 9}, {134, 64, 13, 9}, {148, 64, 12, 9}, {173, 64, 12, 9}, {186, 64, 12, 9}, {199, 64, 13, 9}, {213, 64, 14, 9},
            {228, 64, 12, 9}, {241, 64, 11, 9}, {253, 64, 13, 9}, {267, 64, 13, 9}, {281, 64, 11, 9}, {293, 64, 11, 9}, {305, 64, 12, 9}, {318, 64, 14, 9}, {333, 64, 13, 9}, {347, 64, 13, 9},
            {119, 74, 14, 9}, {134, 74, 13, 9}, {148, 74, 12, 9}, {161, 74, 11, 9}, {173, 74, 12, 9}, {186, 74, 12, 9}, {199, 74, 13, 9}, {213, 74, 14, 9}, {228, 74, 12, 9}, {241, 74, 11, 9},
            {253, 74, 13, 9}, {267, 74, 13, 9}, {281, 74, 11, 9}, {293, 74, 11, 9}, {305, 74, 12, 9}, {318, 74, 14, 9}, {333, 74, 13, 9}, {347, 74, 13, 9}, {119, 84, 14, 9}, {134, 84, 13, 9},
            {148, 84, 12, 9}, {161, 84, 11, 9}, {173, 84, 12, 9}, {186, 84, 12, 9}, {199, 84, 13, 9}, {213, 84, 14, 9}, {228, 84, 12, 9}, {241, 84, 11, 9}, {253, 84, 13, 9}, {267, 84, 13, 9},
            {281, 84, 11, 9}, {293, 84, 11, 9}, {305, 84, 12, 9}, {318, 84, 14, 9}, {333, 84, 13, 9}, {347, 84, 13, 9}, {119, 94, 14, 9}, {134, 94, 13, 9}, {148, 94, 12, 9}, {161, 94, 11, 9},
            {173, 94, 12, 9}, {186, 94, 12, 9}, {199, 94, 13, 9}, {213, 94, 14, 9}, {228, 94, 12, 9}, {241, 94, 11, 9}, {253, 94, 13, 9}, {267, 94, 13, 9}, {281, 94, 11, 9}, {293, 94, 11, 9},
            {305, 94, 12, 9}, {318, 94, 14, 9}, {333, 94, 13, 9}, {347, 94, 13, 9}, {119, 104, 14, 10}, {134, 104, 13, 10}, {148, 104, 12, 10}, {161, 104, 11, 10}, {173, 104, 12, 10}, {186, 104, 12, 10},
            {199, 104, 13, 10}, {213, 104, 14, 10}, {228, 104, 12, 10}, {241, 104, 11, 10}, {253, 104, 13, 10}, {267, 104, 13, 10}, {281, 104, 11, 10}, {293, 104, 11, 10}, {305, 104, 12, 10}, {318, 104, 14, 10},
            {333, 104, 13, 10}, {347, 104, 13, 10}, {119, 115, 14, 10}, {134, 115, 13, 10}, {148, 115, 12, 10}, {161, 115, 11, 10}, {173, 115, 12, 10}, {186, 115, 12, 10}, {199, 115, 13, 10}, {213, 115, 14, 10},
            {228, 115, 12, 10}, {241, 115, 11, 10}, {253, 115, 13, 10}, {267, 115, 13, 10}, {281, 115, 11, 10}, {293, 115, 11, 10}, {305, 115, 12, 10}, {318, 115, 14, 10}, {333, 115, 13, 10}, {347, 115, 13, 10},
            {119, 126, 14, 9}, {134, 126, 13, 9}, {148, 126, 12, 9}, {161, 126, 11, 9}, {173, 126, 12, 9}, {186, 126, 12, 9}, {213, 126, 14, 9}, {228, 126, 12, 9}, {241, 126, 11, 9}, {253, 126, 13, 9},
            {267, 126, 13, 9}, {281, 126, 11, 9}, {293, 126, 11, 9}, {305, 126, 12, 9}, {318, 126, 14, 9}, {333, 126, 13, 9}, {347, 126, 13, 9}, {119, 136, 14, 9}, {134, 136, 13, 9}, {148, 136, 12, 9},
            {161, 136, 11, 9}, {173, 136, 12, 9}, {186, 136, 12, 9}, {199, 136, 13, 9}, {213, 136, 14, 9}, {228, 136, 12, 9}, {241, 136, 11, 9}, {253, 136, 13, 9}, {267, 136, 13, 9}, {281, 136, 11, 9},
            {293, 136, 11, 9}, {305, 136, 12, 9}, {318, 136, 14, 9}, {333, 136, 13, 9}, {347, 136, 13, 9}, {119, 146, 14, 9}, {134, 146, 13, 9}, {148, 146, 12, 9}, {161, 146, 11, 9}, {173, 146, 12, 9},
            {186, 146, 12, 9}, {199, 146, 13, 9}, {213, 146, 14, 9}, {318, 146, 14, 9}, {333, 146, 13, 9}, {347, 146, 13, 9}, {119, 156, 14, 10}, {134, 156, 13, 10}, {148, 156, 12, 10}, {161, 156, 11, 10},
            {173, 156, 12, 10}, {186, 156, 12, 10}, {199, 156, 13, 10}, {213, 156, 14, 10}, {318, 156, 14, 10}, {333, 156, 13, 10}, {347, 156, 13, 10}, {119, 167, 14, 9}, {134, 167, 13, 9}, {148, 167, 12, 9},
            {161, 167, 11, 9}, {173, 167, 12, 9}, {186, 167, 12, 9}, {199, 167, 13, 9}, {213, 167, 14, 9}, {318, 167, 14, 9}, {333, 167, 13, 9}, {347, 167, 13, 9}, {119, 177, 14, 9}, {134, 177, 13, 9},
            {173, 177, 12, 9}, {186, 177, 12, 9}, {199, 177, 13, 9}, {213, 177, 14, 9}, {318, 177, 14, 9}, {333, 177, 13, 9}, {347, 177, 13, 9}, {119, 187, 14, 10}, {134, 187, 13, 10}, {173, 187, 12, 10},
            {186, 187, 12, 10}, {199, 187, 13, 10}, {213, 187, 14, 10}, {318, 187, 14, 10}, {333, 187, 13, 10}, {347, 187, 13, 10}, {119, 198, 14, 10}, {134, 198, 13, 10}, {148, 198, 12, 10}, {161, 198, 11, 10},
            {173, 198, 12, 10}, {186, 198, 12, 10}, {199, 198, 13, 10}, {213, 198, 14, 10}, {318, 198, 14, 10}, {333, 198, 13, 10}, {347, 198, 13, 10}, {119, 209, 14, 10}, {134, 209, 13, 10}, {148, 209, 12, 10},
            {161, 209, 11, 10}, {173, 209, 12, 10}, {186, 209, 12, 10}, {199, 209, 13, 10}, {213, 209, 14, 10}, {318, 209, 14, 10}, {333, 209, 13, 10}, {347, 209, 13, 10}, {119, 220, 14, 9}, {134, 220, 13, 9},
            {148, 220, 12, 9}, {161, 220, 11, 9}, {173, 220, 12, 9}, {186, 220, 12, 9}, {199, 220, 13, 9}, {213, 220, 14, 9}, {228, 220, 12, 9}, {241, 220, 11, 9}, {253, 220, 13, 9}, {267, 220, 13, 9},
            {305, 220, 12, 9}, {318, 220, 14, 9}, {333, 220, 13, 9}, {347, 220, 13, 9}, {119, 230, 14, 10}, {134, 230, 13, 10}, {148, 230, 12, 10}, {161, 230, 11, 10}, {173, 230, 12, 10}, {186, 230, 12, 10},
            {199, 230, 13, 10}, {213, 230, 14, 10}, {228, 230, 12, 10}, {241, 230, 11, 10}, {253, 230, 13, 10}, {267, 230, 13, 10}, {305, 230, 12, 10}, {318, 230, 14, 10}, {333, 230, 13, 10}, {347, 230, 13, 10},
            {119, 241, 14, 9}, {134, 241, 13, 9}, {148, 241, 12, 9}, {161, 241, 11, 9}, {173, 241, 12, 9}, {186, 241, 12, 9}, {199, 241, 13, 9}, {213, 241, 14, 9}, {228, 241, 12, 9}, {241, 241, 11, 9},
            {253, 241, 13, 9}, {267, 241, 13, 9}, {281, 241, 11, 9}, {293, 241, 11, 9}, {305, 241, 12, 9}, {318, 241, 14, 9}, {333, 241, 13, 9}, {347, 241, 13, 9}, {119, 251, 14, 8}, {134, 251, 13, 8},
            {148, 251, 12, 8}, {161, 251, 11, 8}, {173, 251, 12, 8}, {186, 251, 12, 8}, {199, 251, 13, 8}, {213, 251, 14, 8}, {228, 251, 12, 8}, {241, 251, 11, 8}, {253, 251, 13, 8}, {267, 251, 13, 8},
            {281, 251, 11, 8}, {293, 251, 11, 8}, {305, 251, 12, 8}, {318, 251, 14, 8}, {333, 251, 13, 8}, {347, 251, 13, 8}, {119, 260, 14, 9}, {134, 260, 13, 9}, {148, 260, 12, 9}, {161, 260, 11, 9},
            {173, 260, 12, 9}, {186, 260, 12, 9}, {199, 260, 13, 9}, {213, 260, 14, 9}, {228, 260, 12, 9}, {241, 260, 11, 9}, {253, 260, 13, 9}, {267, 260, 13, 9}, {281, 260, 11, 9}, {333, 260, 13, 9},
            {347, 260, 13, 9}, {119, 270, 14, 9}, {134, 270, 13, 9}, {148, 270, 12, 9}, {161, 270, 11, 9}, {173, 270, 12, 9}, {186, 270, 12, 9}, {199, 270, 13, 9}, {213, 270, 14, 9}, {228, 270, 12, 9},
            {241, 270, 11, 9}, {253, 270, 13, 9}, {267, 270, 13, 9}, {281, 270, 11, 9}, {333, 270, 13, 9}, {347, 270, 13, 9}, {119, 280, 14, 9}, {134, 280, 13, 9}, {148, 280, 12, 9}, {161, 280, 11, 9},
            {173, 280, 12, 9}, {186, 280, 12, 9}, {199, 280, 13, 9}, {213, 280, 14, 9}, {228, 280, 12, 9}, {241, 280, 11, 9}, {253, 280, 13, 9}, {267, 280, 13, 9}, {281, 280, 11, 9}, {333, 280, 13, 9},
            {347, 280, 13, 9}, {119, 290, 14, 9}, {134, 290, 13, 9}, {148, 290, 12, 9}, {161, 290, 11, 9}, {173, 290, 12, 9}, {186, 290, 12, 9}, {199, 290, 13, 9}, {213, 290, 14, 9}, {228, 290, 12, 9},
            {241, 290, 11, 9}, {253, 290, 13, 9}, {267, 290, 13, 9}, {281, 290, 11, 9}, {293, 290, 11, 9}, {305, 290, 12, 9}, {318, 290, 14, 9}, {333, 290, 13, 9}, {347, 290, 13, 9}, {119, 300, 14, 10},
            {134, 300, 13, 10}, {148, 300, 12, 10}, {161, 300, 11, 10}, {173, 300, 12, 10}, {186, 300, 12, 10}, {199, 300, 13, 10}, {213, 300, 14, 10}, {228, 300, 12, 10}, {241, 300, 11, 10}, {267, 300, 13, 10},
            {281, 300, 11, 10}, {293, 300, 11, 10}, {305, 300, 12, 10}, {318, 300, 14, 10}, {333, 300, 13, 10}, {347, 300, 13, 10}, {119, 311, 14, 9}, {134, 311, 13, 9}, {148, 311, 12, 9}, {199, 311, 13, 9},
            {213, 311, 14, 9}, {228, 311, 12, 9}, {241, 311, 11, 9}, {253, 311, 13, 9}, {267, 311, 13, 9}, {281, 311, 11, 9}, {293, 311, 11, 9}, {305, 311, 12, 9}, {318, 311, 14, 9}, {333, 311, 13, 9},
            {347, 311, 13, 9}, {119, 321, 14, 10}, {134, 321, 13, 10}, {148, 321, 12, 10}, {199, 321, 13, 10}, {213, 321, 14, 10}, {228, 321, 12, 10}, {241, 321, 11, 10}, {253, 321, 13, 10}, {267, 321, 13, 10},
            {281, 321, 11, 10}, {293, 321, 11, 10}, {305, 321, 12, 10}, {318, 321, 14, 10}, {333, 321, 13, 10}, {347, 321, 13, 10}, {119, 332, 14, 9}, {134, 332, 13, 9}, {148, 332, 12, 9}, {199, 332, 13, 9},
            {213, 332, 14, 9}, {228, 332, 12, 9}, {241, 332, 11, 9}, {253, 332, 13, 9}, {267, 332, 13, 9}, {305, 332, 12, 9}, {318, 332, 14, 9}, {333, 332, 13, 9}, {347, 332, 13, 9}, {119, 342, 14, 9},
            {134, 342, 13, 9}, {148, 342, 12, 9}, {161, 342, 11, 9}, {173, 342, 12, 9}, {186, 342, 12, 9}, {199, 342, 13, 9}, {213, 342, 14, 9}, {228, 342, 12, 9}, {241, 342, 11, 9}, {253, 342, 13, 9},
            {267, 342, 13, 9}, {305, 342, 12, 9}, {318, 342, 14, 9}, {333, 342, 13, 9}, {347, 342, 13, 9}, {119, 352, 14, 9}, {134, 352, 13, 9}, {161, 352, 11, 9}, {173, 352, 12, 9}, {186, 352, 12, 9},
            {199, 352, 13, 9}, {213, 352, 14, 9}, {228, 352, 12, 9}, {241, 352, 11, 9}, {253, 352, 13, 9}, {267, 352, 13, 9}, {281, 352, 11, 9}, {293, 352, 11, 9}, {305, 352, 12, 9}, {318, 352, 14, 9},
            {333, 352, 13, 9}, {347, 352, 13, 9}, {119, 362, 14, 9}, {134, 362, 13, 9}, {148, 362, 12, 9}, {161, 362, 11, 9}, {173, 362, 12, 9}, {186, 362, 12, 9}, {199, 362, 13, 9}, {213, 362, 14, 9},
            {228, 362, 12, 9}, {241, 362, 11, 9}, {253, 362, 13, 9}, {267, 362, 13, 9}, {281, 362, 11, 9}, {293, 362, 11, 9}, {305, 362, 12, 9}, {318, 362, 14, 9}, {333, 362, 13, 9}, {347, 362, 13, 9},
            {119, 372, 14, 10}, {134, 372, 13, 10}, {148, 372, 12, 10}, {161, 372, 11, 10}, {173, 372, 12, 10}, {186, 372, 12, 10}, {199, 372, 13, 10}, {213, 372, 14, 10}, {228, 372, 12, 10}, {241, 372, 11, 10},
            {253, 372, 13, 10}, {267, 372, 13, 10}, {281, 372, 11, 10}, {293, 372, 11, 10}, {305, 372, 12, 10}, {318, 372, 14, 10}, {333, 372, 13, 10}, {347, 372, 13, 10}
        };
    }
}
