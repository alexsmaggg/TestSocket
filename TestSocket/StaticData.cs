using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

namespace TestSocket
{
    class StaticData
    {
        public static ushort[] ports = new ushort[13] { 27681, 27648, 27649, 27650, 27651, 27652, 27653, 27655, 27669, 27670, 27672, 27673,  27682 };   //порты для сокетов
        //public static ushort[] ports = new ushort[1] { 27681 };   //порты для сокетов

        [StructLayout(LayoutKind.Sequential, Pack = 1)]                
        public struct THeader         //Заголовок пакета в UDP модуле обозначен как TZagolovok    12 byte
        {
            public byte NK_Nazn;
            public byte NIzd_Nazn;
            public byte NMash_Nazn;
            public byte NRM_Nazn;
            public byte NMod_Nazn;
            public byte NK_Ist;
            public byte NIzd_Ist; // номер изделия - по Касьяну
            public byte NMash_Ist; // номер машины (напр., N ПУ)
            public byte NRM_Ist; // номер раб. места
            public byte NMod_Ist; // N идентификатора модели 0-сам(не модель), 1-на РТ,
            public byte TypePacket; // Тип пакета (tcp_ по Касьяну)
            public byte KolPacket; // количество пакетов в этом пакете
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TPackageOUT           //Тестовый пакет для отправки целей
        {
            public THeader Header;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 226)]      // 1 byte количество целей,по 45 byte на цель
            public byte[] PackageInf;
        }



        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TPackageIN                   //Входящий пакет. Трудно использовать данную структуру для всего входящего пакета, т.к заранее неизвестна длина  PackageInf.          
        {
            public byte NK_Nazn;
            public byte NIzd_Nazn;
            public byte NMash_Nazn;
            public byte NRM_Nazn;
            public byte NMod_Nazn;

            public byte NK_Ist;
            public byte NIzd_Ist; // номер изделия - по Касьяну
            public byte NMash_Ist; // номер машины (напр., N ПУ)
            public byte NRM_Ist; // номер раб. места
            public byte NMod_Ist; // N идентификатора модели 0-сам(не модель), 1-на РТ,

            public byte TypePacket; // Тип пакета (tcp_ по Касьяну)
            public byte KolPacket; // количество пакетов в этом пакете

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 226)]
            public byte[] PackageInf;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StandingPoint          //Точка стояния комплекса 24 byte
        {
            public byte NMash;              // номер машины 0-МСНР / N ПУ
            public byte RByte1;             // резерв

            public int Xst;                 // координаты точки стояния: X
            public int Yst;                 // Y
            public int Hst;                 //высота
            public short Biss_gi;          // азимут биссектрисы град

            public ushort NZona;           // номер зоны
            public ushort PrPol;           // признак полушария: 1=С, 2=Ю

            public byte ByteEst;           // 0=нет этой машины, 1-есть
            public byte RByte2;           // резерв
            public short HMsnr;           // Высота МСНР
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AzimuthPackage              //Пакет с азимутом 11 byte
        {
            public byte NIzd;                // 1=КО, 2=СО, 7..10= ЗРК1..4
            public byte X_BR;               // БРА-АУ=1 БРБ=2
            public float Normal;            // 29,5 или 45 Номаль?
            public float Asimut;            // Азимут Задачи
            public byte Rotation_Type;      // 0-первичный разворот по азимуту
                                            // 1-доворот по азимуту
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Target                //Пакет с целью 45 byte  
        {
            public ushort Numc;              //номер цели
            public int Xc;     //координаты цели
            public int Yc;     //координаты цели
            public int Hc;   //координаты цели
            public short Vx;
            public short Vy;
            public short Vh;  //вектора цели
            public short Ax;
            public short Ay;
            public short Ah;  //ускорение
            public ushort TNext;           //Добавлено  25.05.2015  // ???
            public byte TipCel;
            public byte TipAP;
            public byte Gos_Prin;              //0 - чужой 1 - свой
            public byte VidCel;
            public byte Oto_cel;        //характеристики цели
            public ushort Upr;
            public byte Har_Ob;
            public byte Tip_Ob;
            public byte SignShum;
            public byte Gr_Cel;
            public short T_st;         //Время старта пассивной помехи
            public short Kod_Marki;    //Код марки цели   02.07.2015
            public short N_g;          //Перегрузка на звене трассы в ед.g (для самолета)
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Targets               
        {
            public byte X_Kol_Cells;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 225)]
            public byte[] X_MCells;
        }

        //Сериализация структур

        public static RawSerializer<THeader> THeaderSerializer = new RawSerializer<THeader>();
        public static RawSerializer<TPackageOUT> TPackageOUTSerializer = new RawSerializer<TPackageOUT>();
        public static RawSerializer<Targets> TargetsSerializer = new RawSerializer<Targets>();
        public static RawSerializer<Target> TargetSerializer = new RawSerializer<Target>();
        public static RawSerializer<StandingPoint> StandingPointSerializer = new RawSerializer<StandingPoint>();
        public static RawSerializer<AzimuthPackage> AzimuthPackageSerializer = new RawSerializer<AzimuthPackage>();

        //Заголовок для теста 
        public static THeader mockHeader_246()
        {
            THeader Header = new THeader();

            Header.NK_Nazn = 1;
            Header.NIzd_Nazn = 2;
            Header.NMash_Nazn = 3;
            Header.NRM_Nazn = 4;
            Header.NMod_Nazn = 5;
            Header.NK_Ist = 6;
            Header.NIzd_Ist = 7;
            Header.NMash_Ist = 8;
            Header.NRM_Ist = 9;
            Header.NMod_Ist = 10;
            Header.TypePacket = 246;
            Header.KolPacket = 1;
            return Header;
        }

        //Цель для теста 
        public static Target mockTarget()
        {
            Target target = new Target();
            target.Numc = 1;
            target.Xc = 10001;
            target.Yc = 20001;
            target.Hc = 7000;
            target.Ax = 50;
            target.Ay = 20;
            target.Ah = 5;
            target.Vx = 100;
            target.Vy = 200;
            target.Vh = 20;
            target.TNext = 1;
            target.TipCel = 1;
            target.TipAP = 1;
            target.Gos_Prin = 1;
            target.VidCel = 1;
            target.Oto_cel = 1;
            target.Upr = 1;
            target.Har_Ob = 1;
            target.Tip_Ob = 1;
            target.SignShum = 2;
            target.Gr_Cel = 1;
            target.T_st = 1;
            target.Kod_Marki = 1;
            target.N_g = 1;
            return target;
        }

        //Цели для теста 
        public static Targets mockTargetsPackage()
        {
            Targets targets = new Targets();
            Target target = mockTarget();
            byte[] targetArr = TargetSerializer.RawSerialize(target);
            byte[] targetsArr = new byte[225];
            targetArr.CopyTo(targetsArr, 0);
            targetArr.CopyTo(targetsArr, 45);
            targetArr.CopyTo(targetsArr, 90);
            targetArr.CopyTo(targetsArr, 135);
            targetArr.CopyTo(targetsArr, 180);
            targets.X_Kol_Cells = 5;
            targets.X_MCells = targetsArr;
            return targets;
        }

        //Создание тестового пакета с целями
        public static byte[] CreateTestPackage_246()
        {
            TPackageOUT tpo = new TPackageOUT();

            StaticData.THeader Header = StaticData.mockHeader_246();
            StaticData.Targets trgs = StaticData.mockTargetsPackage();
            byte[] PackageInf = TargetsSerializer.RawSerialize(trgs);
            tpo.Header = Header;
            tpo.PackageInf = PackageInf;
            return TPackageOUTSerializer.RawSerialize(tpo);
        }

        //Заголовок входящий
        public static void PrintHeader(THeader header)
        {
            Console.WriteLine("Identification destination host");
            Console.WriteLine($"number of complex {header.NK_Nazn}, ");
            Console.WriteLine($"number of item {header.NIzd_Nazn}");
            Console.WriteLine($"number of viechle {header.NMash_Nazn}");
            Console.WriteLine($"number of workplace {header.NRM_Nazn}");
            Console.WriteLine($"Number of id model {header.NMod_Nazn}");
            Console.WriteLine("Identification source host");
            Console.WriteLine($"number of complex {header.NK_Ist}, ");
            Console.WriteLine($"number of item {header.NIzd_Ist}");
            Console.WriteLine($"number of viechle {header.NMash_Ist}");
            Console.WriteLine($"number of workplace {header.NRM_Ist}");
            Console.WriteLine($"Number of id model {header.NMod_Ist}");
            Console.WriteLine($"type packet {header.TypePacket}");
            Console.WriteLine($"quantity {header.KolPacket}");
            Console.WriteLine("------------------------------------------------------------");
        }


        //Цели входящие
        public static void PrintTargets(byte[] packageBuffer)
        {
            Targets targets = TargetsSerializer.RawDeserialize(packageBuffer, 0);         
            Console.WriteLine($"Количество целей: {targets.X_Kol_Cells}\n");
            var subarrays = targets.X_MCells
                    .Select((s, i) => new { Value = s, Index = i })
                    .GroupBy(x => x.Index / 45) // тут 45 - это длинна одного подмассива
                    .Select(grp => grp.Select(x => x.Value).ToArray())
                    .ToArray();

            foreach (byte[] arr in subarrays)
            {
                Target t = TargetSerializer.RawDeserialize(arr, 0);

                Console.WriteLine($"Номер цели  {t.Numc}");
                Console.WriteLine($"Xc  {t.Xc}");
                Console.WriteLine($"Yc  {t.Yc}");
                Console.WriteLine($"Hc  {t.Hc}");
                Console.WriteLine($"Vx  {t.Vx}");
                Console.WriteLine($"Vy  {t.Vy}");
                Console.WriteLine($"Vh  {t.Vh}");
                Console.WriteLine($"Ax  {t.Ax}");
                Console.WriteLine($"Ay  {t.Ay}");
                Console.WriteLine($"Ah  {t.Ah}");
                Console.WriteLine($"TNext  {t.TNext}");
                Console.WriteLine($"Тип Цели  {t.TipCel}");
                Console.WriteLine($"TipAP  {t.TipAP}");
                Console.WriteLine($"Госпринадлежность  {t.Gos_Prin}");
                Console.WriteLine($"Вид цели  {t.VidCel}");
                Console.WriteLine($"Характеристики цели  {t.Oto_cel}");
                Console.WriteLine($"Upr  {t.Upr}");
                Console.WriteLine($"Har_Ob  {t.Har_Ob}");
                Console.WriteLine($"Tip_Ob  {t.Tip_Ob}");
                Console.WriteLine($"SignShum  {t.SignShum}");
                Console.WriteLine($"Gr_Cel  {t.Gr_Cel}");
                Console.WriteLine($"Время старта пассивной помехи  {t.T_st}");
                Console.WriteLine($"Код марки цели  {t.Kod_Marki}");
                Console.WriteLine($"Перегрузка на звене трассы {t.N_g}\n");              
            }
        }

        //Точки стояния входящий пакет нвывод в консоль
        public static void PrintStandingPoint(byte[] packageBuffer)
        {
            StandingPoint point = StandingPointSerializer.RawDeserialize(packageBuffer, 0);
            Console.WriteLine("!!!----ТОЧКА СТОЯНИЯ---!!!");
            Console.WriteLine($"номер машины  {point.NMash}, ");
            Console.WriteLine($"резерв1  {point.RByte1}, ");
            Console.WriteLine($"резерв2  {point.RByte2}, ");
            Console.WriteLine($"X  {point.Xst}, ");
            Console.WriteLine($"Y  {point.Yst}, ");
            Console.WriteLine($"H  {point.Hst}, ");
            Console.WriteLine($"азимут биссектрисы  {point.Biss_gi}, ");
            Console.WriteLine($"азимут биссектрисы  {point.Biss_gi}, ");
            Console.WriteLine($"номер зоны  {point.NZona}, ");
            Console.WriteLine($"признак полушария  {point.PrPol}, ");
            Console.WriteLine($"есть нет  {point.ByteEst}, ");
            Console.WriteLine($"высота МСНР  {point.HMsnr }, ");
        }

        //Азимут входящий пакет нвывод в консоль
        public static void PrintAzimuthPackage(byte[] packageBuffer)
        {
            AzimuthPackage package = AzimuthPackageSerializer.RawDeserialize(packageBuffer, 0);
            Console.WriteLine("!!!----ПАКЕТ ДЛЯ АЗИМУТА----!!!");
            Console.WriteLine($"NIzd  {package.NIzd}, ");
            Console.WriteLine($"X_BR  {package.X_BR}, ");
            Console.WriteLine($"Азимут Задачи Normal  {package.Normal}, ");
            Console.WriteLine($"Азимут Задачи Azimuth  {package.Asimut}, ");
            Console.WriteLine($"Разворот доворот  {package.Rotation_Type}, ");            
        }

        //Прием 246 пакета и вывод в консоль
        public static void TaskProcess_246(byte[] recvBuffer)
        {
            byte[] headerBuffer = recvBuffer.Take(12).ToArray();
            byte[] packageBuffer = recvBuffer.Skip(12).ToArray();
            THeader header = THeaderSerializer.RawDeserialize(headerBuffer, 0);
            //Console.WriteLine($"recvBuffer:   {recvBuffer.Length}");
            //Console.WriteLine($"headerBuffer:   {headerBuffer.Length}");
            //Console.WriteLine($"packageBuffer:   {packageBuffer.Length}");
            if (header.TypePacket == 246) // Пакет с целями
            {
                PrintHeader(header);
                Console.WriteLine("\nЦЕЛИ!");
                try
                {
                    PrintTargets(packageBuffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine("END TASK\n");
        }

        //Прием 241_243 пакета и вывод в консоль
        public static void TaskProcess_241_243(byte[] recvBuffer)
        {
            byte[] headerBuffer = recvBuffer.Take(12).ToArray();
            byte[] packageBuffer = recvBuffer.Skip(12).ToArray();
            THeader header = THeaderSerializer.RawDeserialize(headerBuffer, 0);
            if (header.TypePacket == 241 || header.TypePacket == 243) // Пакет с целями
            {
                //PrintHeader(header);
                //Console.WriteLine($"recvBuffer:   {recvBuffer.Length}");
                //Console.WriteLine($"headerBuffer:   {headerBuffer.Length}");
               // Console.WriteLine($"packageBuffer:   {packageBuffer.Length}");
                try
                {
                    if (header.TypePacket == 241)
                    {
                        PrintAzimuthPackage(packageBuffer);
                    }
                    else
                    {
                        PrintStandingPoint(packageBuffer);
                    }                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine("END TASK\n");
            }
        }

        //Прием пакета и возврат заголовка
        public static byte TaskProcessEasy(byte[] recvBuffer)
        {
            byte[] headerBuffer = recvBuffer.Take(12).ToArray();
            THeader header = THeaderSerializer.RawDeserialize(headerBuffer, 0);
            return header.TypePacket;
        }

        //создать сокет на свобоном IP c портом
        public static UdpClient CreateUdpClient(ushort port)
        {
            IPAddress[] ipv4Addresses = Array.FindAll(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            UdpClient udpClient = new UdpClient();
            bool isSuccess = false;         
            for (int i = 0; i < ipv4Addresses.Length; i++)
            {
                try
                {
                    udpClient.Client.Bind(new IPEndPoint(ipv4Addresses[i], port));
                    isSuccess = true;
                    Console.WriteLine($"Успешно создал сокет {ipv4Addresses[i]}:{port} "); 
                    break;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    continue;
                }              
            }
            if (!isSuccess)
            {
                throw new Exception("Не удалось создать сокет!");
            }
            return udpClient;
        }
    }

    //Сериализация струтктур в буфер и наоборот
    internal class RawSerializer<T>
    {
        public T RawDeserialize(byte[] rawData)
        {
            return RawDeserialize(rawData, 0);
        }

        public T RawDeserialize(byte[] rawData, int position)
        {
            int rawsize = Marshal.SizeOf(typeof(T));
            if (rawsize > rawData.Length)
                return default(T);

            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            T obj = (T)Marshal.PtrToStructure(buffer, typeof(T));
            Marshal.FreeHGlobal(buffer);
            return obj;
        }

        public byte[] RawSerialize(T item)
        {
            int rawSize = Marshal.SizeOf(typeof(T));
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(item, buffer, false);
            byte[] rawData = new byte[rawSize];
            Marshal.Copy(buffer, rawData, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawData;
        }
    }
}
