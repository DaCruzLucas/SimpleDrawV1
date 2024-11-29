using SimpleDrawLib;
using System.Collections.Generic;

namespace SimpleDrawServer
{
    public class DrawingService
    {
        public Dictionary<int, Shape> Shapes = new Dictionary<int, Shape>();

        int id = 0;

        public DrawingService() { }

        public int GetNextId()
        {
            return id++;
        }

        public void AddShape(Shape shape)
        {
            Shapes.Add(shape.Id, shape);
            Console.WriteLine($"Shape added: {shape.Id}");
        }

        public void RemoveShape(int shapeId)
        {
            Shapes.Remove(shapeId);
            Console.WriteLine($"Shape removed: {shapeId}");
        }

        public void ModifyShape(Shape shape)
        {
            Shapes[shape.Id] = shape;
            Console.WriteLine($"Shape modified: {shape.Id}");
        }

        public void ClearShapes()
        {
            Shapes.Clear();
            Console.WriteLine("Shapes cleared");
        }

        public List<Shape> GetShapes()
        {
            Console.WriteLine("Shapes requested");
            return Shapes.Values.ToList();
        }
    }
}
