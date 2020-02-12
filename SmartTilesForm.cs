﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MLLE
{
    public partial class SmartTilesForm : Form
    {
        J2TFile Tileset;
        bool Result = false;
        ushort[][] TileAssignments; //very temporary, this should be a class
        public SmartTilesForm()
        {
            InitializeComponent();
        }
        internal bool ShowForm(J2TFile tileset)
        {
            Tileset = tileset;
            CreateImageFromTileset();

            //todo
            TileAssignments = new ushort[100][];
            for (var i = 0; i < TileAssignments.Length; ++i)
                TileAssignments[i] = new ushort[0];
            TileAssignments[1] = new ushort[3] { 1, 2, 3 };

            new System.Threading.Timer(RedrawTiles, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.5));
            ShowDialog();
            return Result;
        }

        int elapsed = 0;
        static Rectangle RectangleFromTileID(int tileID) {
            return new Rectangle((tileID % 10) * 32, (tileID / 10) * 32, 32, 32);
        }
        private void RedrawTiles(object state)
        {
            var image = smartPicture.Image;

            using (Graphics graphics = Graphics.FromImage(image))
                for (int i = 0; i < TileAssignments.Length; ++i) {
                    ushort[] assignment = TileAssignments[i];
                    if (assignment.Length > 0)
                    {
                        graphics.DrawImage(tilesetPicture.Image, RectangleFromTileID(i), RectangleFromTileID(assignment[elapsed % assignment.Length]), GraphicsUnit.Pixel);
                    }
                }
            smartPicture.Image = image;

            ++elapsed;
        }

        private void CreateImageFromTileset()
        {
            //there are enough windows that show tileset images you'd think I should turn some/all of this into a method somewhere
            tilesetPicture.Height = (int)(Tileset.TotalNumberOfTiles + 9) / 10 * 32;
            var image = new Bitmap(tilesetPicture.Width, tilesetPicture.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] bytes = new byte[data.Height * data.Stride];
            //Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            for (uint i = 0; i < Tileset.TotalNumberOfTiles; ++i)
            {
                var tileImage = Tileset.Images[Tileset.ImageAddress[i]];
                var xOrg = (i % 10) * 32;
                var yOrg = i / 10 * 32;
                for (uint x = 0; x < 32; ++x)
                    for (uint y = 0; y < 32; ++y)
                    {
                        bytes[xOrg + x + (yOrg + y) * data.Stride] = tileImage[x + y*32];
                    }
            }
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            image.UnlockBits(data);

            var palette = image.Palette;
            var entries = palette.Entries;
            for (uint i = 0; i < Palette.PaletteSize; ++i)
                entries[i] = Palette.Convert(Tileset.Palette.Colors[i]);
            image.Palette = palette;

            tilesetPicture.Image = image;

            smartPicture.Image = Properties.Resources.SmartTilesPermutations;
        }
        
        private void OKButton_Click(object sender, EventArgs e)
        {
            Result = true;
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
        
        int GetMouseTileIDFromTileset(MouseEventArgs e)
        {
            var pictureOrigin = tilesetPicture.AutoScrollOffset;
            return ((e.X - pictureOrigin.X) / 32 + (e.Y - pictureOrigin.Y) / 32 * 10);
        }
        Point GetPointFromTileID(int tileID, int xAdjust)
        {
            return new Point(
                tilesetPicture.Left + (tileID % 10) * 32,
                tilesetPicture.Top + (tileID / 10) * 32
            );
        }


        private void tilesetPicture_MouseMove(object sender, MouseEventArgs e)
        {
            Text = "Define Smart Tiles \u2013 " + GetMouseTileIDFromTileset(e);
        }

        private void tilesetPicture_MouseLeave(object sender, EventArgs e)
        {
            Text = "Define Smart Tiles";
        }

        private void tilesetPicture_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void smartPicture_Click(object sender, EventArgs e)
        {

        }
    }
}
