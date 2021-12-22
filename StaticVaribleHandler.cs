using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVaribleHandler : MonoBehaviour
{
    public static string currentPatient = "";
    public static string currentKinectCode = "";
    public static string currentPatientCode = "";

    public static bool isEndStatic = false;
    public static bool isEndDynamic = false;


    public static int WIDTH = 52;
    public static int HEIGHT = 44;
    public static int SCALE = 10;

    public static int MaximumStaticFrameCount = 100;
    //가로줄 배열(52)
    public static int[] widthMappingArray = new int[] { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 51, 49, 47, 45, 43, 41, 39, 37, 35, 33, 31, 29, 27, 25, 23, 21, 19, 17, 15, 13, 11, 9, 7, 5, 3, 1 };
    //public static int[] widthMappingArray = new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37, 39, 41, 43, 45, 47, 49, 51, 50, 48, 46, 44, 42, 40, 38, 36, 34, 32, 30, 28, 26, 24, 22, 20, 18, 16, 14, 12, 10, 8, 6, 4, 2, 0 };
    //세로줄 배열(44)
    public static int[] heightMappingArray = new int[] { 21, 22, 20, 23, 19, 24, 18, 25, 17, 26, 16, 27, 15, 28, 14, 29, 13, 30, 12, 31, 11, 32, 10, 33, 9, 34, 8, 35, 7, 36, 6, 37, 5, 38, 4, 39, 3, 40, 2, 41, 1, 42, 0, 43 };
    //public static int[] heightMappingArray = new int[] {43,0,42,1,41,2,40,3,39,4,38,5,37,6,36,7,35,8,34,9,33,10,32,11,31,12,30,13,29,14,28,15,27,16,26,17,25,18,24,19,23,20,22,21 };


    public static Queue<int> SearchPixel(int START_IDX, Color32[] detectedImage)
    {
        Queue<int> bfsQueue = new Queue<int>();
        Queue<int> visitQueue = new Queue<int>();

        bfsQueue.Enqueue(START_IDX);

        while (bfsQueue.Count != 0)
        {
            int idx = bfsQueue.Dequeue();   //pop and push visited

            for (int i = 0; i < 5; i++)
            {
                int leftmod = idx % WIDTH - i;
                int rightmod = idx % WIDTH + i;

                int right = idx + i;
                if (right >= detectedImage.Length)
                {
                    //out of index DO NOTHING
                }
                else if (leftmod < 0 || rightmod >= WIDTH)
                {
                    //out of index DO NOTHING
                }
                else
                {
                    if (detectedImage[right].r == 255 && detectedImage[right].g == 255 && detectedImage[right].b == 255)
                    {
                        //default pixel DO NOTHING
                    }
                    else
                    {
                        if (visitQueue.Contains(right))
                        {
                            //bfs exists DO NOTHING
                        }
                        else
                        {
                            bfsQueue.Enqueue(right);
                            visitQueue.Enqueue(right);
                        }
                    }
                }

                int left = idx - i;
                if (left < 0)
                {
                    //out of index DO NOTHING
                }
                else if (leftmod < 0 || rightmod >= WIDTH)
                {
                    //out of index DO NOTHING
                }
                else
                {
                    if (detectedImage[left].r == 255 && detectedImage[left].g == 255 && detectedImage[left].b == 255)
                    {
                        //default pixel DO NOTHING
                    }
                    else
                    {
                        if (visitQueue.Contains(left))
                        {
                            //bfs exists DO NOTHING
                        }
                        else
                        {
                            bfsQueue.Enqueue(left);
                            visitQueue.Enqueue(left);
                        }
                    }
                }
                for (int j = 0; j < 3; j++)
                {
                    int bot = idx + (i + 1 + j * 5) * WIDTH;

                    if (bot >= detectedImage.Length)
                    {
                        //out of index DO NOTHING
                    }
                    else if (leftmod < 0)
                    {
                        //out of index DO NOTHING
                    }
                    else
                    {
                        if (detectedImage[bot].r == 255 && detectedImage[bot].g == 255 && detectedImage[bot].b == 255)
                        {
                            //default pixel DO NOTHING
                        }
                        else
                        {
                            if (visitQueue.Contains(bot))
                            {
                                //bfs exists DO NOTHING
                            }
                            else
                            {
                                bfsQueue.Enqueue(bot);
                                visitQueue.Enqueue(bot);
                            }
                        }
                    }
                }
            }

            //left top
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int leftmod = idx % WIDTH - k;

                    int lefttop = idx - j * WIDTH - k;

                    if (lefttop >= detectedImage.Length)
                    {
                        //out of index DO NOTHING
                    }
                    else if (leftmod < 0 || lefttop < 0)
                    {
                        //out of index DO NOTHING
                    }
                    else
                    {
                        if (lefttop < 0)
                        {
                            Debug.Log(lefttop);
                        }
                        if (detectedImage[lefttop].r == 255 && detectedImage[lefttop].g == 255 && detectedImage[lefttop].b == 255)
                        {
                            //default pixel DO NOTHING
                        }
                        else
                        {
                            if (visitQueue.Contains(lefttop))
                            {
                                //bfs exists DO NOTHING
                            }
                            else
                            {
                                bfsQueue.Enqueue(lefttop);
                                visitQueue.Enqueue(lefttop);
                            }
                        }
                    }
                }
            }
            //right top
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int rightmod = idx % WIDTH + k;

                    int righttop = idx - j * WIDTH + k;

                    if (righttop >= detectedImage.Length)
                    {
                        //out of index DO NOTHING
                    }
                    else if (rightmod >= WIDTH || righttop < 0)
                    {
                        //out of index DO NOTHING
                    }
                    else
                    {
                        if (detectedImage[righttop].r == 255 && detectedImage[righttop].g == 255 && detectedImage[righttop].b == 255)
                        {
                            //default pixel DO NOTHING
                        }
                        else
                        {
                            if (visitQueue.Contains(righttop))
                            {
                                //bfs exists DO NOTHING
                            }
                            else
                            {
                                bfsQueue.Enqueue(righttop);
                                visitQueue.Enqueue(righttop);
                            }
                        }
                    }
                }
            }

            //left bot
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int leftmod = idx % WIDTH - k;

                    int leftbot = idx + j * WIDTH - k;

                    if (leftbot >= detectedImage.Length)
                    {
                        //out of index DO NOTHING
                    }
                    else if (leftmod < 0)
                    {
                        //out of index DO NOTHING
                    }
                    else
                    {
                        if (detectedImage[leftbot].r == 255 && detectedImage[leftbot].g == 255 && detectedImage[leftbot].b == 255)
                        {
                            //default pixel DO NOTHING
                        }
                        else
                        {
                            if (visitQueue.Contains(leftbot))
                            {
                                //bfs exists DO NOTHING
                            }
                            else
                            {
                                bfsQueue.Enqueue(leftbot);
                                visitQueue.Enqueue(leftbot);
                            }
                        }
                    }
                }
            }

            //right bot
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int rightbot = idx + j * WIDTH + k;
                    int rightmod = idx % WIDTH + k;

                    if (rightbot >= detectedImage.Length)
                    {
                        //out of index DO NOTHING
                    }
                    else if (rightmod >= WIDTH)
                    {
                        //out of index DO NOTHING
                    }
                    else
                    {
                        if (detectedImage[rightbot].r == 255 && detectedImage[rightbot].g == 255 && detectedImage[rightbot].b == 255)
                        {
                            //default pixel DO NOTHING
                        }
                        else if (rightbot < WIDTH * HEIGHT / 2 && idx % WIDTH >= 3)
                        {
                            if (visitQueue.Contains(rightbot))
                            {
                                //bfs exists DO NOTHING
                            }
                            else
                            {
                                bfsQueue.Enqueue(rightbot);
                                visitQueue.Enqueue(rightbot);
                            }
                        }
                    }
                }
            }

        }
        return visitQueue;
    }

    public static Tuple<int, int, int, int> getLongestPixel (Queue<int> q, Color32[] detectedImage)
    {
        //var detectedImage = imageDetected.sprite.texture.GetPixels32();
        bool[] idxArray = new bool[WIDTH];
        int[] maxHeightArray = new int[WIDTH];
        int max = 0;
        int maxidx = 0;
        int startidx = 999;
        int endidx = 0;

        foreach (int idx in q)
        {
            int mod = idx % WIDTH;
            idxArray[mod] = true;
        }

        for (int i = 0; i < WIDTH; i++)
        {
            int height = 0;
            int start = -1;
            int end = -1;
            if (idxArray[i]) //해당 세로줄에 발견된 픽셀이 존재한다
            {
                start = -1;
                end = -1;
                for (int j = 0; j < HEIGHT; j++)
                {
                    if (detectedImage[j * WIDTH + i].r == 255 && detectedImage[j * WIDTH + i].g == 255 && detectedImage[j * WIDTH + i].b == 255)
                    {
                        //DEFAULT PIXEL
                    }
                    else
                    {
                        if (start == -1)
                        {
                            start = j;
                        }
                        end = j;
                    }
                }
            }
            height = end - start;
            if (max < height)
            {
                max = height;
                maxidx = i;
                //startidx = start;
                //endidx = end;
            }
            if(startidx > start && height != 0)
            {
                startidx = start;
            }
            if(endidx < end && height != 0)
            {
                endidx = end;
            }
        }

        return new Tuple<int, int, int, int>(max, maxidx, startidx, endidx);
    }

    public static int DetectToe(Color32[] detectedImage, int startidx, List<int> toeList)
    {
        Queue<int> detectedQueue = new Queue<int>();
        Queue<int> visitQueue = new Queue<int>();

        detectedQueue.Enqueue(startidx);
        visitQueue.Enqueue(startidx);

        while (detectedQueue.Count != 0)
        {
            int idx = detectedQueue.Dequeue();   //pop and push visited

            for (int i = 0; i < 1; i++)
            {
                int right = idx + i;
                int rightmod = idx % WIDTH + i;
                if (right >= detectedImage.Length && rightmod >= WIDTH)
                {
                    //out of index DO NOTHING
                }
                else
                {
                    if (detectedImage[right].r == 255 && detectedImage[right].g == 255 && detectedImage[right].b == 255)
                    {
                        //default pixel DO NOTHING
                    }
                    else
                    {
                        if (visitQueue.Contains(right))
                        {
                            //bfs exists DO NOTHING
                        }
                        else if (toeList.Contains(right))
                        {
                            detectedQueue.Enqueue(right);
                            visitQueue.Enqueue(right);
                        }
                    }
                }

                int left = idx - i;
                int leftmod = idx % WIDTH - i;
                if (left <= 0 && leftmod < 0)
                {
                    //out of index DO NOTHING
                }
                else
                {
                    if (detectedImage[left].r == 255 && detectedImage[left].g == 255 && detectedImage[left].b == 255)
                    {
                        //default pixel DO NOTHING
                    }
                    else
                    {
                        if (visitQueue.Contains(left))
                        {
                            //bfs exists DO NOTHING
                        }
                        else if (toeList.Contains(left))
                        {
                            detectedQueue.Enqueue(left);
                            visitQueue.Enqueue(left);
                        }
                    }
                }
            }
            int bottom = idx + WIDTH;
            if (bottom >= detectedImage.Length)
            {
                //out of index DO NOTHING
            }
            else
            {
                if (detectedImage[bottom].r == 255 && detectedImage[bottom].g == 255 && detectedImage[bottom].b == 255)
                {
                    //default pixel DO NOTHING
                }
                else
                {
                    if (visitQueue.Contains(bottom))
                    {
                        //bfs exists DO NOTHING
                    }
                    else if (toeList.Contains(bottom))
                    {
                        detectedQueue.Enqueue(bottom);
                        visitQueue.Enqueue(bottom);
                    }
                }
            }

            int top = idx - WIDTH;
            if (top < 0)
            {
                //out of index DO NOTHING
            }
            else
            {
                if (detectedImage[top].r == 255 && detectedImage[top].g == 255 && detectedImage[top].b == 255)
                {
                    //default pixel DO NOTHING
                }
                else
                {
                    if (visitQueue.Contains(top))
                    {
                        //bfs exists DO NOTHING
                    }
                    else if (toeList.Contains(top))
                    {
                        detectedQueue.Enqueue(top);
                        visitQueue.Enqueue(top);
                    }
                }
            }
        }
        return visitQueue.Count;
    }
}
