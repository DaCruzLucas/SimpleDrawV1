using Microsoft.AspNetCore.SignalR.Client;
using SimpleDrawLib;
using System.Diagnostics;
using System.Drawing;

namespace SimpleDraw
{
    public partial class DrawingForm : Form
    {
        HubConnection connection;

        List<Shape> Shapes = new List<Shape>();
        Shape selectedShape;

        List<ToolStripButton> Tools = new List<ToolStripButton>();
        ToolStripButton SelectedTool;

        private int PrevMouseX;
        private int PrevMouseY;

        int selectedPoint = 0;

        int handleSize = 5;

        int id;

        public DrawingForm()
        {
            InitializeComponent();

            connection = new HubConnectionBuilder()
                .WithUrl("http://pc-bd18-17:5000/draw")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<int[][]>("ReceiveOnConnectedAsync", (shapes) =>
            {
                for (int i = 0; i < shapes.Length; i++)
                {
                    Shapes.Add(CreateShape(shapes[i][0], (Shape.ShapeType)shapes[i][1], shapes[i][2], shapes[i][3], shapes[i][4], shapes[i][5], shapes[i][6]));
                }

                DrawingPanel.Invalidate();
            });

            connection.On("ReceiveNewShape", (int id, Shape.ShapeType type, int x1, int y1, int x2, int y2, int argb) =>
            {
                Debug.WriteLine($"Received new shape with ID: {id} ({x1}, {y1}) to ({x2}, {y2}) {type} {Color.FromArgb(argb)}");
                Shapes.Add(CreateShape(id, type, x1, y1, x2, y2, argb));
                DrawingPanel.Invalidate();
            });

            connection.On<int>("ReceiveRemoveShape", (shapeId) =>
            {
                Debug.WriteLine($"Received remove shape with ID: {shapeId}");
                if (selectedShape != null && selectedShape.Id == shapeId) selectedShape = null;
                Shapes.RemoveAll(s => s.Id == shapeId);
                DrawingPanel.Invalidate();
            });

            connection.On("ReceiveModifyShape", (int id, Shape.ShapeType type, int x1, int y1, int x2, int y2, int argb) =>
            {
                //Debug.WriteLine($"Received modify shape with ID: {id} ({x1}, {y1}) to ({x2}, {y2}) {type} {Color.FromArgb(argb)}");
                var s = Shapes.Find(shape => shape.Id == id);
                if (s != null)
                {
                    if (s == selectedShape)
                    {
                        FgColorButton.SelectedColor = Color.FromArgb(argb);
                    }

                    s.Type = type;
                    s.X1 = x1;
                    s.Y1 = y1;
                    s.X2 = x2;
                    s.Y2 = y2;
                    s.LineColor = Color.FromArgb(argb);
                    DrawingPanel.Invalidate();
                }
            });

            connection.On("ReceiveClearShapes", () =>
            {
                selectedShape = null;
                Shapes.Clear();
                DrawingPanel.Invalidate();
            });

            connection.On<int>("ReceiveNextId", (id) =>
            {
                this.id = id;
            });

            connection.StartAsync();

            Tools.Add(LineTool);
            Tools.Add(RectangleTool);
            Tools.Add(EllipseTool);
            Tools.Add(PointerTool);

            SelectTool(LineTool);
        }

        void SelectTool(ToolStripItem tool)
        {
            foreach (var t in Tools)
            {
                if (t.Checked = t == tool)
                {
                    SelectedTool = t;
                }
            }

            if (SelectedTool != null)
            {
                SelectedTool.Checked = true;
            }
        }

        private bool isHandleHit(int mouseX, int mouseY, int x, int y)
        {
            if (mouseX >= x - handleSize && mouseX <= x + handleSize && mouseY >= y - handleSize && mouseY <= y + handleSize)
            {
                return true;
            }

            return false;
        }

        private void DrawHandle(Graphics g, int x, int y)
        {
            g.FillRectangle(Brushes.White, x - handleSize / 2, y - handleSize / 2, handleSize, handleSize);
            g.DrawRectangle(Pens.Black, x - handleSize / 2, y - handleSize / 2, handleSize, handleSize);
        }

        private Shape SelectShape(int x, int y)
        {
            for (int i = Shapes.Count - 1; i >= 0; i--)
            {
                if (Shapes[i].IsHit(x, y))
                {
                    return Shapes[i];
                }
            }

            return null;
        }

        private async void DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (SelectedTool == PointerTool)
                {
                    if (selectedShape != null && isHandleHit(e.X, e.Y, selectedShape.X1, selectedShape.Y1))
                    {
                        selectedPoint = 1;
                    }
                    else if (selectedShape != null && isHandleHit(e.X, e.Y, selectedShape.X2, selectedShape.Y2))
                    {
                        selectedPoint = 2;
                    }
                    else if (selectedShape != null && isHandleHit(e.X, e.Y, selectedShape.X2, selectedShape.Y1))
                    {
                        selectedPoint = 3;
                    }
                    else if (selectedShape != null && isHandleHit(e.X, e.Y, selectedShape.X1, selectedShape.Y2))
                    {
                        selectedPoint = 4;
                    }
                    else
                    {
                        selectedShape = SelectShape(e.X, e.Y);

                        if (selectedShape != null)
                        {
                            selectedPoint = 0;
                            FgColorButton.SelectedColor = selectedShape.LineColor;

                            Debug.WriteLine($"Selected shape: {selectedShape.Id}");
                        }
                        else
                        {
                            selectedPoint = -1;
                        }

                        PrevMouseX = e.X;
                        PrevMouseY = e.Y;
                    }
                }
                else
                {
                    await GetNextId();

                    if (SelectedTool == LineTool)
                    {
                        selectedShape = new Line();
                        selectedShape.Type = Shape.ShapeType.Line;
                    }
                    else if (SelectedTool == RectangleTool)
                    {
                        selectedShape = new SimpleDrawLib.Rectangle();
                        selectedShape.Type = Shape.ShapeType.Rectangle;
                    }
                    else if (SelectedTool == EllipseTool)
                    {
                        selectedShape = new Ellipse();
                        selectedShape.Type = Shape.ShapeType.Ellipse;
                    }

                    selectedShape.Id = id;

                    selectedShape.X1 = selectedShape.X2 = e.X;
                    selectedShape.Y1 = selectedShape.Y2 = e.Y;
                    selectedShape.LineColor = FgColorButton.SelectedColor;

                    //Shapes.Add(selectedShape);
                    await SendNewShape(selectedShape);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                selectedShape = null;
                await SendClearShapes();
            }

            DrawingPanel.Invalidate();
        }

        private async void DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (SelectedTool == PointerTool)
                {
                    if (selectedShape != null)
                    {
                        if (selectedPoint == 1)
                        {
                            selectedShape.X1 = e.X;
                            selectedShape.Y1 = e.Y;
                        }
                        else if (selectedPoint == 2)
                        {
                            selectedShape.X2 = e.X;
                            selectedShape.Y2 = e.Y;
                        }
                        else if (selectedPoint == 3)
                        {
                            selectedShape.X2 = e.X;
                            selectedShape.Y1 = e.Y;
                        }
                        else if (selectedPoint == 4)
                        {
                            selectedShape.X1 = e.X;
                            selectedShape.Y2 = e.Y;
                        }
                        else
                        {
                            int dx = e.X - PrevMouseX;
                            int dy = e.Y - PrevMouseY;

                            selectedShape.X1 += dx;
                            selectedShape.Y1 += dy;
                            selectedShape.X2 += dx;
                            selectedShape.Y2 += dy;

                            PrevMouseX = e.X;
                            PrevMouseY = e.Y;
                        }
                    }
                }
                else if (selectedShape != null)
                {
                    selectedShape.X2 = e.X;
                    selectedShape.Y2 = e.Y;
                }

                if (selectedShape != null)
                {
                    await SendModifyShape(selectedShape);
                }
            }

            DrawingPanel.Invalidate();
        }

        private void DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            
        }

        private async void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            if (selectedShape != null && selectedShape.LineColor != FgColorButton.SelectedColor)
            {
                selectedShape.LineColor = FgColorButton.SelectedColor;
                await SendModifyShape(selectedShape);
            }

            for (int i = 0; i < Shapes.Count; i++)
            {
                Shapes[i].Draw(e.Graphics);
            }

            if (selectedShape != null)
            {
                DrawHandle(e.Graphics, selectedShape.X1, selectedShape.Y1);
                DrawHandle(e.Graphics, selectedShape.X2, selectedShape.Y2);

                if (selectedShape.GetType() != typeof(Line))
                {
                    DrawHandle(e.Graphics, selectedShape.X2, selectedShape.Y1);
                    DrawHandle(e.Graphics, selectedShape.X1, selectedShape.Y2);
                }
            }
        }

        private void DrawingToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            SelectTool(e.ClickedItem);
        }

        private async void DrawingForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (selectedShape != null)
                {
                    //Shapes.Remove(selectedShape);
                    await SendRemoveShape(selectedShape.Id);

                    selectedShape = null;
                    
                    DrawingPanel.Invalidate();
                }
            }
        }

        private void DrawingForm_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private async Task GetNextId()
        {
            await connection.InvokeAsync("GetNextId");
        }

        private async Task SendNewShape(Shape shape)
        {
            int argb = shape.LineColor.ToArgb();
            await connection.SendAsync("SendNewShape", shape.Id, shape.Type, shape.X1, shape.Y1, shape.X2, shape.Y2, argb);
        }

        private async Task SendRemoveShape(int shapeId)
        {
            await connection.SendAsync("SendRemoveShape", shapeId);
        }

        private async Task SendModifyShape(Shape shape)
        {
            int argb = shape.LineColor.ToArgb();
            await connection.SendAsync("SendModifyShape", shape.Id, shape.Type, shape.X1, shape.Y1, shape.X2, shape.Y2, argb);
        }

        private async Task SendClearShapes()
        {
            await connection.SendAsync("SendClearShapes");
        }

        private async Task SendGetShapes()
        {
            await connection.SendAsync("SendGetShapes");
        }

        private Shape CreateShape(int id, Shape.ShapeType type, int x1, int y1, int x2, int y2, int argb)
        {
            Shape shape;

            if (type == Shape.ShapeType.Line)
            {
                shape = new Line
                {
                    Id = id,
                    Type = type,
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    LineColor = Color.FromArgb(argb)
                };
            }
            else if (type == Shape.ShapeType.Rectangle)
            {
                shape = new SimpleDrawLib.Rectangle
                {
                    Id = id,
                    Type = type,
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    LineColor = Color.FromArgb(argb)
                };
            }
            else if (type == Shape.ShapeType.Ellipse)
            {
                shape = new Ellipse
                {
                    Id = id,
                    Type = type,
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    LineColor = Color.FromArgb(argb)
                };
            }
            else
            {
                shape = null;
            }

            return shape;
        }
    }
}
