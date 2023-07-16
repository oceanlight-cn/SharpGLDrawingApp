using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingApp
{
    public class LayerManager
    {
        private static LayerManager instance;
        private List<Layer> layers;
        private int nextLayerId;

        public static LayerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LayerManager();
                }
                return instance;
            }
        }

        private LayerManager()
        {
            layers = new List<Layer>();
            nextLayerId = 1;
        }

        public Layer AddLayer()
        {
            Layer layer = new Layer(nextLayerId);
            layers.Add(layer);
            CurrentLayerId = nextLayerId;
            nextLayerId++;
            return layer;
        }

        public void HideLayer(int layerId)
        {
            Layer layer = GetLayerById(layerId);
            if (layer != null)
            {
                layer.Visible = false;
            }
        }

        public void RemoveLayer(int layerId)
        {
            Layer layer = GetLayerById(layerId);
            if (layer != null)
            {
                layer.Removed = true;
            }
        }


        public void ShowLayer(int layerId)
        {
            Layer layer = GetLayerById(layerId);
            if (layer != null)
            {
                layer.Visible = true;
            }
        }

        public Layer GetLayerById(int layerId)
        {
            return layers.Find(layer => layer.Id == layerId);
        }

        private int GenerateId()
        {
            return nextLayerId++;
        }

        public int CurrentLayerId { get; set; }
    }

    public class Layer
    {
        public int Id { get; }
        public bool Visible { get; set; }

        public bool Removed { get; set; }

        public Layer(int id)
        {
            Id = id;
            Visible = true;
            Removed = false;
        }
    }
}

