using Microsoft.AspNetCore.SignalR;
using SimpleDrawLib;
using System.Drawing;

namespace SimpleDrawServer
{
    public class DrawHub : Hub
    {
        private readonly DrawingService ds;

        public DrawHub(DrawingService drawingService)
        {
            ds = drawingService;
        }

        public override async Task OnConnectedAsync()
        {
            int[][] Shapes = ds.GetShapes().Select(shape => new int[] { shape.Id, (int)shape.Type, shape.X1, shape.Y1, shape.X2, shape.Y2, shape.LineColor.ToArgb() }).ToArray();

            await Clients.Caller.SendAsync("ReceiveOnConnectedAsync", Shapes);

            await base.OnConnectedAsync();
        }

        public async Task GetNextId()
        {
            await Clients.Caller.SendAsync("ReceiveNextId", ds.GetNextId());
        }

        public Shape CreateShape(int id, Shape.ShapeType type, int x1, int y1, int x2, int y2, int argb)
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

        public async Task SendNewShape(int id, Shape.ShapeType type, int x1, int y1, int x2, int y2, int argb)
        {
            try
            {
                Shape shape = CreateShape(id, type, x1, y1, x2, y2, argb);

                if (shape == null)
                {
                    return;
                }

                ds.AddShape(shape);

                await Clients.All.SendAsync("ReceiveNewShape", id, type, x1, y1, x2, y2, argb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task SendRemoveShape(int shapeId)
        {
            ds.RemoveShape(shapeId);

            await Clients.All.SendAsync("ReceiveRemoveShape", shapeId);
        }

        public async Task SendModifyShape(int id, Shape.ShapeType type, int x1, int y1, int x2, int y2, int argb)
        {
            Shape shape = CreateShape(id, type, x1, y1, x2, y2, argb);

            if (shape == null)
            {
                return;
            }

            ds.ModifyShape(shape);

            await Clients.All.SendAsync("ReceiveModifyShape", id, type, x1, y1, x2, y2, argb);
        }

        public async Task SendClearShapes()
        {
            //ds.ClearShapes();
            //await Clients.All.SendAsync("ReceiveClearShapes");
        }

        public async Task SendGetShapes()
        {
            await Clients.Caller.SendAsync("ReceiveGetShapes", ds.GetShapes());
        }
    }
}
