using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace _300RublesForCube
{
   public class Cube
   {
      public Vector3[] vertices;
      public Vector2[] textureCoordinates;
      public Vector3[] normals;
      public int[][] faces;
      public float Size { get; private set; }

      private Bitmap bitmap;
      private int textureID;

      public Cube(float size)
      {
         Size = size;
         float hs = size / 2;

         vertices = new Vector3[8];
         faces = new int[6][];

         vertices[0] = new Vector3(-hs, -hs, +hs);
         vertices[1] = new Vector3(-hs, -hs, -hs);
         vertices[2] = new Vector3(+hs, -hs, -hs);
         vertices[3] = new Vector3(+hs, -hs, +hs);

         vertices[4] = new Vector3(-hs, +hs, +hs);
         vertices[5] = new Vector3(-hs, +hs, -hs);
         vertices[6] = new Vector3(+hs, +hs, -hs);
         vertices[7] = new Vector3(+hs, +hs, +hs);

         faces[0] = new int[4] { 0, 1, 5, 4 };
         faces[1] = new int[4] { 1, 2, 6, 5 };
         faces[2] = new int[4] { 2, 3, 7, 6 };
         faces[3] = new int[4] { 3, 0, 4, 7 };
         faces[4] = new int[4] { 0, 3, 2, 1 };
         faces[5] = new int[4] { 4, 5, 6, 7 };
      }

      public void CalcNormals()
      {
         normals = new Vector3[6];

         for (int i = 0; i < 6; i++)
         {
            Vector3 v0 = vertices[faces[i][0]];
            Vector3 v1 = vertices[faces[i][1]];
            Vector3 v2 = vertices[faces[i][2]];

            Vector3 u = v0 - v1;
            Vector3 v = v0 - v2;

            Vector3 normal = new Vector3(
               u.Y * v.Z - u.Z * v.Y,
               u.Z * v.X - u.X * v.Z,
               u.X * v.Y - u.Y * v.X
               );

            normals[i] = -1 * normal.Normalized();
         }
      }

      public void DrawMesh()
      {
         GL.Begin(PrimitiveType.Quads);

         for (int i = 0; i < 6; i++)
         {
            for (int j = 0; j < 4; j++)
            {
               GL.Vertex3(vertices[faces[i][j]]);
            }
         }

         GL.End();
      }

      public void DrawMeshWithNormals()
      {
         GL.Begin(PrimitiveType.Quads);

         for (int i = 0; i < 6; i++)
         {
            GL.Normal3(normals[i]);

            for (int j = 0; j < 4; j++)
            {
               GL.Vertex3(vertices[faces[i][j]]);
            }
         }

         GL.End();
      }

      public void CalcTextureCoords()
      {
         textureCoordinates = new Vector2[] {
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1) };
      }

      public void DrawTexture()
      {
         GL.BindTexture(TextureTarget.Texture2D, textureID);

         GL.Enable(EnableCap.Texture2D);
         GL.Begin(PrimitiveType.Quads);

         for (int i = 0; i < 6; i++)
         {
            for (int j = 0; j < 4; j++)
            {
               GL.Vertex3(vertices[faces[i][j]]);
               GL.TexCoord2(textureCoordinates[j]);
            }
         }

         GL.End();
         GL.Disable(EnableCap.Texture2D);
      }

      public void DrawTextureWithNormals()
      {
         GL.BindTexture(TextureTarget.Texture2D, textureID);

         GL.Enable(EnableCap.Texture2D);
         GL.Begin(PrimitiveType.Quads);

         for (int i = 0; i < 6; i++)
         {
            GL.Normal3(normals[i]);

            for (int j = 0; j < 4; j++)
            {
               GL.Vertex3(vertices[faces[i][j]]);
               GL.TexCoord2(textureCoordinates[j]);
            }
         }

         GL.End();
         GL.Disable(EnableCap.Texture2D);
      }

      public void DrawNormals()
      {
         GL.Begin(PrimitiveType.Lines);

         for (int i = 0; i < 6; i++)
         {
            for (int j = 0; j < 4; j++)
            {
               Vector3 start = vertices[faces[i][j]];
               Vector3 end = start + normals[i];

               GL.Vertex3(start);
               GL.Vertex3(end);
            }

         }

         GL.End();
      }

      public void DrawGrid()
      {
         GL.Begin(PrimitiveType.Lines);

         for (int i = 0; i < 6; i++)
         {
            for (int j = 0; j < 4; j++)
            {
               int next = (j + 1) % 4;

               GL.Vertex3(vertices[faces[i][j]]);
               GL.Vertex3(vertices[faces[i][next]]);
            }
         }

         GL.End();
      }

      public void ReadTexture(string fileName)
      {
         bitmap = new Bitmap(fileName);
      }

      public void BindTexture()
      {
         GL.GenTextures(1, out textureID);

         GL.BindTexture(TextureTarget.Texture2D, textureID);

         BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
           ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

         GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
             OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

         bitmap.UnlockBits(data);

         //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, 6);

         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
      }

   }
}
