﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography;
using System.IO;

namespace LightShot_Parser
{
    static partial class ParserMethods
    {
        public static int imgAmount = 0; // Количество загружаемых скриншотов
        public static int faultvalue = 0; // Количество ошибок при работе программы
        public static int ExeptionCount = 0; // Количество фатальных ошибок

        // Все возможные символы в ссылке на скриншот
        public static char[] allowedChars = "abcdefghijklmnopqrstuvwxyz123456789".ToCharArray();
        public static bool HashCheck(string localFileName, int i)
        {
            const string HEX = "9B5936F4006146E4E1E9025B474C02863C0B5614132AD40DB4B925A10E8BFBB9";
            string hash = null;

            using (FileStream stream = File.OpenRead((localFileName + (i + 1) + ".png")))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                hash = BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
            
            if(hash == HEX)
            {
                File.Delete((localFileName + (i + 1) + ".png"));
                Message.WarningExpect("Данный скриншот удален");
            }

            return true;
        }

        public static void AlgoritmParse(WebClient client, Regex reHref, string localFileName)
        {
            Console.Write("Введите количество скриншотов, которые необходимо загрузить [не более 99.999]: ");
            imgAmount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();

            Console.Write("Введите первую цифру ссылки [она влияет на год создания скриншота 1-9]: ");
            string src_upd = Console.ReadLine();
            Console.WriteLine();

            int totalDownload = 0;

            for (int i = 0; i < allowedChars.Length; i++)
            {
                for(int j = 0; j < allowedChars.Length; j++)
                {
                    for(int p = 0; p < allowedChars.Length; p++)
                    {
                        for(int g = 0; g < allowedChars.Length; g++)
                        {
                            for(int a = 0; a < allowedChars.Length; a++)
                            {
                                string lightShot_src = "https://prnt.sc/" + src_upd + allowedChars[i]
                                + allowedChars[j] + allowedChars[p] + allowedChars[g] + allowedChars[a];

                                // Парсинг HTML-кода страницы
                                Uri uri = new Uri(lightShot_src);
                                string html = client.DownloadString(uri);

                                try
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Количество скаченных скриншотов: " + (totalDownload + 1));
                                    Console.WriteLine("Ссылка на скриншот: " + lightShot_src);
                                    Console.WriteLine("Текущий скриншот для скачивания: " + reHref.Match(html));

                                    // Непосредственно парсинг картинки и добавление ее в соответсвующую директорию
                                    client.DownloadFile((reHref.Match(html)).ToString(), (localFileName + (totalDownload + 1) + ".png"));
                                    HashCheck(localFileName, totalDownload);
                                    
                                    client.Headers["User-Agent"] = "Mozilla/5.0";
                                    Thread.Sleep(100);
                                    
                                    if(imgAmount == totalDownload + 1) Message.FinishingMessage();
                                    totalDownload++;

                                }
                                catch (System.ArgumentException) // Обработчик исключений, если не удалось получить доступ к картинке
                                {
                                    Message.WarningExpect("Не удалось загрузить данный скриншот");

                                    client.Headers["User-Agent"] = "Mozilla/5.0";
                                    faultvalue++;
                                    continue;
                                }

                                catch (System.Net.WebException) // Обработчик исключений, если указан не верный абсолютный путь
                                {
                                    ExeptionCount++;
                                    Console.Clear();
                                    Message.FaultExpect("Папка не найдена, пожалуйста, введите правильный путь следуя примеру");
                                    break;
                                }

                             

                            }
                        }
                    }
                }
            }
        }

        public static void RandomParse(WebClient client, Regex reHref, string localFileName)
        {
            Console.Write("Введите количество скриншотов, которые необходимо загрузить [не более 99.999]: ");
            imgAmount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();

            Random random = new Random();

            for (int i = 0; i < imgAmount; i++)
            {
                // Генерация рандомной ссылки
                string src_upd = random.Next(1, 9).ToString();
                for (int j = 0; j < 5; j++)
                {
                    src_upd += allowedChars[random.Next(0, 34)];
                }

                string lightShot_src = "https://prnt.sc/" + src_upd;

                // Парсинг HTML-кода страницы
                Uri uri = new Uri(lightShot_src);
                string html = client.DownloadString(uri);

                try
                {
                    Console.WriteLine();
                    Console.WriteLine("Количество скаченных скриншотов: " + (i + 1));
                    Console.WriteLine("Ссылка на скриншот: " + lightShot_src);
                    Console.WriteLine("Текущий скриншот для скачивания: " + reHref.Match(html));

                    // Непосредственно парсинг картинки и добавление ее в соответсвующую директорию
                    client.DownloadFile((reHref.Match(html)).ToString(), (localFileName + (i + 1) + ".png"));
                    HashCheck(localFileName, i);
        
                    client.Headers["User-Agent"] = "Mozilla/5.0";
                    Thread.Sleep(100);
                }
                catch (System.ArgumentException) // Обработчик исключений, если не удалось получить доступ к картинке
                {
                    Message.WarningExpect("Не удалось загрузить данный скриншот");

                    client.Headers["User-Agent"] = "Mozilla/5.0";
                    faultvalue++;
                    continue;
                }

                catch (System.Net.WebException) // Обработчик исключений, если указан не верный абсолютный путь
                {
                    ExeptionCount++;
                    Console.Clear();
                    Message.FaultExpect("Папка не найдена, пожалуйста, введите правильный путь следуя примеру");
                    break;
                }

            }

            if (ExeptionCount == 0) Message.FinishingMessage();

        }

        public static void NumberParse(WebClient client, Regex reHref, string localFileName)
        {
            Console.Write("Введите количество скриншотов, которые необходимо загрузить [не более 99.999]: ");
            imgAmount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();

            Console.Write("Введите первую цифру ссылки [она влияет на год создания скриншота 1-9]: ");
            int src_upd = Convert.ToInt32(Console.ReadLine()) * 100000;
            Console.WriteLine();

            for (int i = 0; i < imgAmount; i++)
            {
                // Создание ссылки для парсинга
                string lightShot_src = "https://prnt.sc/" + src_upd++;
                Uri uri = new Uri(lightShot_src);
                // Парсинг HTML-кода сайта
                string html = client.DownloadString(uri);

                try
                {
                    Console.WriteLine();
                    Console.WriteLine("Количество скаченных скриншотов: " + (i + 1));
                    Console.WriteLine("Ссылка на скриншот: " + lightShot_src);
                    Console.WriteLine("Текущий скриншот для скачивания: " + reHref.Match(html));

                    // Непосредственно парсинг изображения и установка в соответсвущую директорию
                    client.DownloadFile((reHref.Match(html)).ToString(), (localFileName + (i + 1) + ".png"));
                    HashCheck(localFileName, i);
                    
                    client.Headers["User-Agent"] = "Mozilla/5.0";
                    Thread.Sleep(100);
                }
                catch (System.ArgumentException) // Обработчик исключений, если не удалось получить доступ к картинке
                {
                    Message.WarningExpect("Не удалось загрузить данный скриншот");

                    client.Headers["User-Agent"] = "Mozilla/5.0";
                    faultvalue++;
                    continue;
                }

                catch (System.Net.WebException) // Обработчик исключений, если указан не верный путь 
                {
                    ExeptionCount++;
                    Console.Clear();
                    Message.FaultExpect("Папка не найдена, пожалуйста, введите правильный путь следуя примеру");
                    break;
                }


            }

            if (ExeptionCount == 0) Message.FinishingMessage();

        }
    }
}
