using UnityEngine;

namespace TradeProof.Environment
{
    public class RoomBuilder : MonoBehaviour
    {
        public GameObject BuildRoom(RoomDefinition def)
        {
            GameObject room = new GameObject("JobSite_Room");

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(room.transform);
            floor.transform.localPosition = new Vector3(0, -0.05f, 0);
            floor.transform.localScale = new Vector3(def.width, 0.1f, def.length);
            floor.GetComponent<Renderer>().material.color = def.floorColor;

            // Ceiling
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(room.transform);
            ceiling.transform.localPosition = new Vector3(0, def.height + 0.05f, 0);
            ceiling.transform.localScale = new Vector3(def.width, 0.1f, def.length);
            ceiling.GetComponent<Renderer>().material.color = def.ceilingColor;

            // Walls
            CreateWall(room, "WallNorth", new Vector3(0, def.height / 2f, def.length / 2f),
                new Vector3(def.width, def.height, 0.1f), def.wallColor);
            CreateWall(room, "WallSouth", new Vector3(0, def.height / 2f, -def.length / 2f),
                new Vector3(def.width, def.height, 0.1f), def.wallColor);
            CreateWall(room, "WallEast", new Vector3(def.width / 2f, def.height / 2f, 0),
                new Vector3(0.1f, def.height, def.length), def.wallColor);
            CreateWall(room, "WallWest", new Vector3(-def.width / 2f, def.height / 2f, 0),
                new Vector3(0.1f, def.height, def.length), def.wallColor);

            // Baseboard trim
            float baseboardHeight = 0.08f;
            CreateTrim(room, "BaseboardN", new Vector3(0, baseboardHeight / 2f, def.length / 2f - 0.05f),
                new Vector3(def.width - 0.2f, baseboardHeight, 0.02f), new Color(0.9f, 0.9f, 0.88f));
            CreateTrim(room, "BaseboardS", new Vector3(0, baseboardHeight / 2f, -def.length / 2f + 0.05f),
                new Vector3(def.width - 0.2f, baseboardHeight, 0.02f), new Color(0.9f, 0.9f, 0.88f));
            CreateTrim(room, "BaseboardE", new Vector3(def.width / 2f - 0.05f, baseboardHeight / 2f, 0),
                new Vector3(0.02f, baseboardHeight, def.length - 0.2f), new Color(0.9f, 0.9f, 0.88f));
            CreateTrim(room, "BaseboardW", new Vector3(-def.width / 2f + 0.05f, baseboardHeight / 2f, 0),
                new Vector3(0.02f, baseboardHeight, def.length - 0.2f), new Color(0.9f, 0.9f, 0.88f));

            // Directional light for the room
            GameObject lightObj = new GameObject("RoomLight");
            lightObj.transform.SetParent(room.transform);
            lightObj.transform.localPosition = new Vector3(0, def.height - 0.1f, 0);
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light roomLight = lightObj.AddComponent<Light>();
            roomLight.type = LightType.Directional;
            roomLight.intensity = 0.8f;
            roomLight.color = new Color(1f, 0.97f, 0.92f);

            // Point light for ambient
            GameObject pointLightObj = new GameObject("AmbientLight");
            pointLightObj.transform.SetParent(room.transform);
            pointLightObj.transform.localPosition = new Vector3(0, def.height - 0.3f, 0);
            Light pointLight = pointLightObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.range = Mathf.Max(def.width, def.length) * 1.5f;
            pointLight.intensity = 0.5f;
            pointLight.color = new Color(1f, 0.95f, 0.9f);

            Debug.Log($"[RoomBuilder] Built room: {def.width}x{def.length}x{def.height}m");
            return room;
        }

        private void CreateWall(GameObject parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent.transform);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material.color = color;
        }

        private void CreateTrim(GameObject parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = name;
            trim.transform.SetParent(parent.transform);
            trim.transform.localPosition = position;
            trim.transform.localScale = scale;
            trim.GetComponent<Renderer>().material.color = color;

            // Remove collider from trim (decorative only)
            Collider col = trim.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }
    }
}
