namespace SimpleDraw
{
    public class ToolStripColorButton : ToolStripButton
    {
        private Color selectedColor;

        public Color SelectedColor 
        { 
            get
            {
                return selectedColor;
            }

            set
            {
                selectedColor = value;
                UpdateImage();
            }
        }

        public System.Drawing.Rectangle ColorRectangle { get; set; } = new(0, 13, 16, 3);

        private void UpdateImage()
        {
            try
            {
                Graphics g = Graphics.FromImage(Image);
                g.FillRectangle(new SolidBrush(selectedColor), ColorRectangle);
                Invalidate();
            }
            catch
            {

            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            ColorDialog colorDialog = new ColorDialog();

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                SelectedColor = colorDialog.Color;
                Form.ActiveForm.Refresh();
            }
        }
    }
}
