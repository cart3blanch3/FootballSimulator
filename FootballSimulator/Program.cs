using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml;
using System.Text.Json;

namespace FootballSimulator
{
    // Базовый абстрактный класс игрока
    public abstract class Player
    {
        public string Name { get; set; } // Имя игрока
        public int ShirtNumber { get; set; } // Номер на майке игрока
        public int Scores { get; set; } // Общее количество очков игрока
        public bool HasRedCard { get; set; } // Флаг, указывающий наличие красной карточки у игрока
        public int YellowCards { get; set; } // Счетчик желтых карточек у игрока

        public Player(string name, int shirtNumber)
        {
            Name = name;
            ShirtNumber = shirtNumber;
            Scores = 0; // Начальное количество очков устанавливается в 0
            HasRedCard = false; // Игрок начинает без красной карточки
            YellowCards = 0; // Игрок начинает без желтых карточек
        }

        public Player()
        {
            // Конструктор без параметров для сериализации
        }

        public abstract void Play(); // Абстрактный метод, представляющий действие игрока в игре
        // Каждый наследник класса Player должен реализовать свою версию метода Play, в которой описано его уникальное поведение в игре.

        public void Score()
        {
            Scores++; // Увеличивает общее количество очков игрока
        }

        public void ReceiveYellowCard()
        {
            YellowCards++; // Увеличивает количество желтых карточек у игрока

            if (YellowCards == 2)
            {
                HasRedCard = true; // Если игрок получает две желтые карточки, устанавливается флаг наличия красной карточки
            }
        }

        public void ReceiveRedCard()
        {
            HasRedCard = true; // Устанавливает флаг наличия красной карточки у игрока
        }
    }

    // Дочерний класс нападающего, наследующийся от базового Player
    public class Forward : Player
    {
        public Forward(string name, int shirtNumber) : base(name, shirtNumber)
        {
            // Конструктор с параметрами, вызывает конструктор базового класса
        }

        public Forward()
        {
            // Конструктор без параметров для сериализации
        }

        public override void Play()
        {
            Console.WriteLine($"Нападающий {Name} с номером {ShirtNumber} атакует!");
            // Переопределение абстрактного метода Play в классе Forward.
            // Выводится информация о действии нападающего при выполнении метода.
        }
    }

    // Дочерний класс вратаря, наследующийся от базового Player
    public class Goalkeeper : Player
    {

        public Goalkeeper(string name, int shirtNumber) : base(name, shirtNumber)
        {
            // Конструктор с параметрами, вызывает конструктор базового класса
        }

        public Goalkeeper()
        {
            // Конструктор без параметров для сериализации
        }

        public override void Play()
        {
            Console.WriteLine($"Вратарь {Name} с номером {ShirtNumber} защищает ворота!");
            // Переопределение абстрактного метода Play в классе Goalkeeper.
            // Выводится информация о действии вратаря при выполнении метода.
        }
    }
    [Serializable] // Атрибут указывает, что объекты этого класса могут быть сериализованы
    [XmlInclude(typeof(Forward))] // Указывает, что в XML-сериализации должен учитываться тип Forward при сериализации Team
    [XmlInclude(typeof(Goalkeeper))] // Указывает, что в XML-сериализации должен учитываться тип Goalkeeper при сериализации Team
    public class Team<TPlayer> : IEnumerable<TPlayer> where TPlayer : Player
    {
        public string Name { get; set; } // Имя команды
        public List<TPlayer> StartingPlayers { get; set; } // Список игроков в основном составе
        public List<TPlayer> SubstitutePlayers { get; set; } // Список запасных игроков

        public Team(string name)
        {
            Name = name; // Инициализация имени команды
            StartingPlayers = new List<TPlayer>(); // Инициализация списка основного состава
            SubstitutePlayers = new List<TPlayer>(); // Инициализация списка запасных игроков
        }

        // Конструктор без параметров для сериализации
        public Team()
        {
            StartingPlayers = new List<TPlayer>(); // Инициализация списка основного состава
            SubstitutePlayers = new List<TPlayer>(); // Инициализация списка запасных игроков
        }

        // Метод добавления игрока
        public void Add(TPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }
            if (StartingPlayers.Count >= 11)
            {
                SubstitutePlayers.Add(player); // Если основной состав полон, добавляем игрока в запасных
            }
            else if (player is Goalkeeper && StartingPlayers.Any(p => p is Goalkeeper))
            {
                SubstitutePlayers.Add(player); // Если игрок вратарь и уже есть вратарь в основном составе, добавляем его в запасных
            }
            else
            {
                StartingPlayers.Add(player); // Иначе добавляем игрока в основной состав
            }
        }

        // Метод замены игрока
        public bool SubstitutePlayer(TPlayer player)
        {
            // Проверяем, что игрок находится в основном составе и есть запасные игроки
            if (StartingPlayers.Contains(player) && SubstitutePlayers.Count > 0)
            {
                // Получаем первого доступного запасного игрока
                var replacement = SubstitutePlayers.FirstOrDefault();

                // Если найден заменяющий игрок
                if (replacement != null)
                {
                    // Заменяем игрока в основном составе
                    SubstitutePlayers.Remove(replacement);
                    StartingPlayers.Remove(player);

                    SubstitutePlayers.Add(player);
                    StartingPlayers.Add(replacement);

                    // Выводим сообщение о замене
                    Console.WriteLine($"Игрок {player.Name} заменен игроком {replacement.Name} в команде {Name}.");
                    return true;
                }
                else
                {
                    // Если не найден заменяющий игрок, выбрасываем исключение
                    throw new FootballException($"Нет подходящей замены для игрока {player.Name} в команде {Name}.");
                }
            }
            else
            {
                // Если условия для замены не выполняются, выводим сообщение об ошибке
                Console.WriteLine("Невозможно провести замену. Проверьте наличие игроков в основном составе и запасных.");
                return false;
            }
        }

        // Метод вывода информации о команде
        public void PrintTeamInfo()
        {
            Console.WriteLine($"Команда: {Name}"); 
            Console.WriteLine("Основной состав:");

            // Вывод информации об игроках в основном составе
            foreach (var player in StartingPlayers)
            {
                Console.WriteLine($"  {player.Name} - {player.GetType().Name} (Номер: {player.ShirtNumber}, Очки: {player.Scores}");
            }

            Console.WriteLine("Запасные игроки:");

            // Вывод информации о запасных игроках
            foreach (var player in SubstitutePlayers)
            {
                Console.WriteLine($"  {player.Name} - {player.GetType().Name} (Номер: {player.ShirtNumber}, Очки:  {player.Scores}");
            }
        }


        // Реализация интерфейса IEnumerable
        public IEnumerator<TPlayer> GetEnumerator()
        {
            return StartingPlayers.Concat(SubstitutePlayers).GetEnumerator(); // Объединение списков итераторов основного состава и запасных
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(); // Возвращение итератора для основного состава и запасных
        }

        // Асинхронная сортировка основного состава с использованием делегата Action
        public async Task SortStartingPlayersAsync(Action<List<TPlayer>> sortAction)
        {
            await Task.Run(() => sortAction(StartingPlayers)); // Асинхронный запуск сортировки основного состава
        }

        // Асинхронная сортировка запасных игроков с использованием делегата Action
        public async Task SortSubstitutePlayersAsync(Action<List<TPlayer>> sortAction)
        {
            await Task.Run(() => sortAction(SubstitutePlayers)); // Асинхронный запуск сортировки запасных игроков
        }

        // Сравнение двух игроков с использованием делегата Func
        public int ComparePlayers(TPlayer player1, TPlayer player2, Func<TPlayer, TPlayer, int> comparison)
        {
            return comparison(player1, player2); // Вызов делегата для сравнения игроков
        }
    }


    public class Commentator
    {
        // Статическое поле Random для генерации случайных чисел
        protected static readonly Random Random = new Random();

        // Имя комментатора
        public string Name { get; private set; }

        // Конструктор класса Commentator
        public Commentator(string name)
        {
            Name = name;
        }

        // Метод для обработки событий матча
        public void HandleMatchEvent(object sender, string matchEvent)
        {
            Console.WriteLine($"{Name}: {matchEvent}");
        }
    }

    public class SportsJournalist
    {
        // Объект блокировки для управления доступом к файлу из разных потоков
        private readonly object fileLock = new object();

        // Имя журналиста
        private string journalistName;

        // Путь к файлу для записи событий
        private string filePath;

        // Конструктор класса SportsJournalist
        public SportsJournalist(string name, string filePath)
        {
            journalistName = name;
            this.filePath = filePath;
        }

        // Метод для отчета о событии матча
        public void ReportMatchEvent(object sender, string matchEvent)
        {
            // Формирование строки с информацией о событии
            string logMessage = $"{DateTime.Now} - {sender.GetType().Name}: {matchEvent}";

            // Запись события в файл с использованием блокировки
            WriteToLogFile(logMessage);
        }

        // Приватный метод для записи в файл события матча
        private void WriteToLogFile(string logMessage)
        {
            try
            {
                // Использование блокировки для предотвращения одновременной записи из разных потоков
                lock (fileLock)
                {
                    // Запись в файл, добавляя новые записи в конец файла
                    File.AppendAllText(filePath, logMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // Обработка и вывод ошибки при записи в файл
                throw new FileLoadException(ex.Message);
            }
        }
    }


    // Перечисление типов карточек (желтая, красная)
    public enum CardType
    {
        Yellow,
        Red
    }

    // Класс, представляющий нарушение (фол) в матче
    public class Foul
    {
        // Имя игрока, совершившего нарушение
        public string PlayerName { get; set; }

        // Название команды игрока
        public string TeamName { get; set; }

        // Признак наличия карточки у игрока
        public bool HasCard { get; set; }

        // Тип карточки (желтая или красная)
        public CardType CardType { get; set; }

        // Признак наличия штрафного удара
        public bool HasPenalty { get; set; }

        // Признак наличия свободного удара
        public bool HasFreeKick { get; set; }

        // Конструктор класса Foul, инициализирующий основные параметры
        public Foul(string playerName, string teamName)
        {
            PlayerName = playerName;
            TeamName = teamName;
            HasCard = false;
            HasPenalty = false;
            HasFreeKick = false;
        }
    }

    // Класс судьи, ответственного за назначение нарушений и карточек
    public class Referee
    {
        // Имя судьи
        public string Name { get; private set; }

        // Конструктор класса Referee, устанавливающий имя судьи
        public Referee(string name)
        {
            Name = name;
        }

        // Метод для вызова нарушения (фола) в матче
        public void CallFoul(Foul foul)
        {
            // Выдача карточки (желтая или красная) с определенной вероятностью
            foul.HasCard = true;
            foul.CardType = (FootballMatch.Random.Next(20) == 0) ? CardType.Red : CardType.Yellow; // Шанс красной: 5%, желтой: 95%

            // Решение по предоставлению штрафного удара
            if (FootballMatch.Random.Next(2) == 0)
            {
                foul.HasFreeKick = true;
            }

            // Решение по предоставлению пенальти
            if (!foul.HasFreeKick && FootballMatch.Random.Next(2) == 0)
            {
                foul.HasPenalty = true;
            }
            // Если уже назначен штрафной удар, то пенальти не назначаем
        }
    }

    public class FootballMatch
    {
        // Статическое поле Random
        public static readonly Random Random = new Random();

        // Команды, участвующие в матче
        public Team<Player> FirstTeam { get; set; }
        public Team<Player> SecondTeam { get; set; }

        // Счет команд
        public int FirstTeamScore { get; private set; }
        public int SecondTeamScore { get; private set; }

        // Комментатор, журналист и судья матча
        public Commentator MatchCommentator { get; private set; }
        public SportsJournalist MatchJournalist { get; private set; }
        public Referee MatchReferee { get; private set; }

        // Переменные для отслеживания технического поражения и победителя
        private bool technicalDefeat = false;
        private string winner;

        // Событие матча, которое уведомляет подписчиков о событиях
        public event EventHandler<string> MatchEvent;

        // Метод для логирования событий матча
        private void LogMatchEvent(string message)
        {
            // Вызываем событие, чтобы оповестить всех подписчиков
            MatchEvent?.Invoke(this, message);
        }

        // Конструктор класса, инициализирующий команды и участников матча
        public FootballMatch(Team<Player> firstTeam, Team<Player> secondTeam, Commentator commentator, Referee referee, SportsJournalist journalist)
        {
            FirstTeam = firstTeam;
            SecondTeam = secondTeam;
            FirstTeamScore = 0;
            SecondTeamScore = 0;
            MatchCommentator = commentator;
            MatchReferee = referee;
            MatchJournalist = journalist;

            // Подписываем комментатора и журналиста на события матча
            MatchEvent += commentator.HandleMatchEvent;
            MatchEvent += journalist.ReportMatchEvent;
        }

        // Метод для симуляции всего матча
        public async Task SimulateMatch()
        {
            // Проверка наличия команд для проведения матча
            if (FirstTeam == null || SecondTeam == null)
            {
                throw new FootballException("Недостаточно команд для проведения матча.");
            }

            // Логирование начала матча
            LogMatchEvent($"\nНачало матча: {FirstTeam.Name} против {SecondTeam.Name}");

            // Симуляция двух таймов
            await SimulateHalf(FirstTeam, SecondTeam);
            if (CheckTechnicalDefeat())
            {
                // Выйти из метода, если было техническое поражение
                UnsubscribeFromEvents();
                return;
            }
            LogMatchEvent("\nПерерыв");
            await SimulateHalf(SecondTeam, FirstTeam);
            if (CheckTechnicalDefeat())
            {
                // Выйти из метода, если было техническое поражение
                UnsubscribeFromEvents();
                return;
            }

            // Проверка на ничью после основного времени
            if (FirstTeamScore == SecondTeamScore)
            {
                LogMatchEvent("Начало овертайма");
                // Назначаем овертайм
                LogMatchEvent("Начало овертайма: 1");
                await SimulateOvertime();
                if (CheckTechnicalDefeat())
                {
                    // Выйти из метода, если было техническое поражение
                    UnsubscribeFromEvents();
                    return;
                }

                LogMatchEvent("Перерыв в овертайме");

                LogMatchEvent("Начало овертайма: 2");
                await SimulateOvertime();
            }

            // Определение победителя
            UnsubscribeFromEvents();
            string winner = (FirstTeamScore > SecondTeamScore) ? FirstTeam.Name : SecondTeam.Name;
            LogMatchEvent($"Конец матча: {winner} побеждает со счетом {FirstTeamScore} : {SecondTeamScore}\n");
        }

        // Метод для проверки технического поражения
        private bool CheckTechnicalDefeat()
        {
            if (technicalDefeat)
            {
                // Выйти из метода, если было техническое поражение
                string winner = (FirstTeamScore > SecondTeamScore) ? FirstTeam.Name : SecondTeam.Name;
                LogMatchEvent($"Конец матча: {winner} побеждает со счетом {FirstTeamScore} : {SecondTeamScore}\n");
                return true;
            }
            return false;
        }

        // Метод для отписки от событий комментатора и журналиста
        private void UnsubscribeFromEvents()
        {
            MatchEvent -= MatchCommentator.HandleMatchEvent;
            MatchEvent -= MatchJournalist.ReportMatchEvent;
        }

        // Метод симуляции тайма заданной длительности ( в минутах)
        private async Task SimulateHalfAsync(Team<Player> attackingTeam, Team<Player> defendingTeam, int duration)
        {
            // Цикл иммитации минут матча
            for (int i = 0; i < duration; i++)
            {
                Console.WriteLine($"\nМинута {i + 1}:");

                // Рандомно выбираем атакующую команду
                Team<Player> currentAttackingTeam = (Random.Next(2) == 0) ? attackingTeam : defendingTeam;

                // Рандомно выбираем нападающего из атакующей команды
                var attackingForward = currentAttackingTeam.OfType<Forward>().OrderBy(x => Random.Next()).FirstOrDefault();

                if (attackingForward != null)
                {
                    attackingForward.Play();

                    // Рандомно определяем, произошло ли нарушение
                    if (Random.Next(10) == 0) // Шанс нарушения: 10%
                    {
                        // Рандомно выбираем команду
                        Team<Player> foulingTeam = (Random.Next(2) == 0) ? attackingTeam : defendingTeam;

                        // Рандомно выбираем нападающего из выбранной команды
                        var foulingForward = foulingTeam.OfType<Forward>().OrderBy(x => Random.Next()).FirstOrDefault();

                        if (foulingForward != null)
                        {
                            // Теперь передаем название команды нарушителя
                            Foul foul = new Foul(foulingForward.Name, foulingTeam.Name);
                            MatchReferee.CallFoul(foul);
                            LogMatchEvent($"Нарушение: {foulingTeam.Name} - {foulingForward.Name}");

                            if (foul.HasCard)
                            {
                                if (foul.CardType == CardType.Red)
                                {
                                    LogMatchEvent($"Красная карточка: {foulingTeam.Name} - {foulingForward.Name}");
                                    // Игрок получил красную карточку, проверяем возможность замены
                                    if (!foulingTeam.SubstitutePlayer(foulingForward))
                                    {
                                        // Замена невозможна, команда получает техническое поражение
                                        LogMatchEvent($"Техническое поражение: {foulingTeam.Name}");
                                        // Обнуление очков проигравшей команды
                                        if (foulingTeam == FirstTeam)
                                        {
                                            SecondTeamScore = 1;
                                            FirstTeamScore = 0;
                                        }
                                        else
                                        {
                                            FirstTeamScore = 1;
                                            SecondTeamScore = 0;
                                        }
                                        technicalDefeat = true;
                                        LogMatchEvent($"Конец матча: {winner} техническим поражением");
                                        return;
                                    }
                                    else
                                    {
                                        LogMatchEvent($"Игрок заменен, матч продолжается.");
                                    }
                                }
                                else if (foul.CardType == CardType.Yellow)
                                {
                                    // Игрок получил желтую карточку
                                    foulingForward.ReceiveYellowCard();
                                    LogMatchEvent($"Желтая карточка: {foulingTeam.Name} - {foulingForward.Name}");

                                    if (foulingForward.YellowCards == 2)
                                    {
                                        LogMatchEvent($"Вторая желтая карточка: {foulingTeam.Name} - {foulingForward.Name}");
                                        // Проверяем возможность замены после второй желтой карточки
                                        if (!foulingTeam.SubstitutePlayer(foulingForward))
                                        {
                                            // Замена невозможна, команда получает техническое поражение
                                            LogMatchEvent($"Техническое поражение: {foulingTeam.Name}");
                                            // Обнуление очков проигравшей команды
                                            if (foulingTeam == FirstTeam)
                                            {
                                                SecondTeamScore = 1;
                                                FirstTeamScore = 0;
                                            }
                                            else
                                            {
                                                FirstTeamScore = 1;
                                                SecondTeamScore = 0;
                                            }
                                            technicalDefeat = true;
                                            LogMatchEvent($"Конец матча: {winner} техническим поражением");
                                            return;
                                        }
                                        else
                                        {
                                            LogMatchEvent($"Игрок заменен, матч продолжается.");
                                        }
                                    }
                                }
                            }

                            if (foul.HasFreeKick)
                            {
                                LogMatchEvent($"Штрафной удар: {foul.TeamName}");
                                if (Random.Next(2) == 0) // Шанс реализации штрафного удара: 50%
                                {
                                    LogMatchEvent($"Гол: {attackingTeam.Name} - {attackingForward.Name}");
                                    attackingForward.Score();
                                    SecondTeamScore++;
                                    LogMatchEvent($"Счет: {FirstTeam.Name} {FirstTeamScore} : {SecondTeam.Name} {SecondTeamScore}");
                                }
                                else
                                {
                                    LogMatchEvent($"Заблокирован штрафной удар: {defendingTeam.Name}");
                                }
                            }

                            if (foul.HasPenalty)
                            {
                                LogMatchEvent($"Пенальти: {foul.TeamName}");
                                if (Random.Next(2) == 0) // Шанс реализации пенальти: 50%
                                {
                                    LogMatchEvent($"Гол: {attackingTeam.Name} - {attackingForward.Name}");
                                    attackingForward.Score();
                                    SecondTeamScore++;
                                    LogMatchEvent($"Счет: {FirstTeam.Name} {FirstTeamScore} : {SecondTeam.Name} {SecondTeamScore}");
                                }
                                else
                                {
                                    LogMatchEvent($"Заблокировано пенальти: {defendingTeam.Name}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Рандомно определяем, забил ли гол
                        if (Random.Next(5) == 0)
                        {
                            attackingForward.Score();
                            LogMatchEvent($"Гол: {attackingTeam.Name} - {attackingForward.Name}");

                            // Начисляем очко команде, которая забила гол
                            if (currentAttackingTeam == FirstTeam)
                            {
                                FirstTeamScore++;
                            }
                            else
                            {
                                SecondTeamScore++;
                            }

                            // Объявляем текущий счет
                            LogMatchEvent($"Счет: {FirstTeam.Name} {FirstTeamScore} : {SecondTeam.Name} {SecondTeamScore}");
                        }
                        else
                        {
                            // Защита вратаря другой команды
                            var defendingGoalkeeper = defendingTeam.OfType<Goalkeeper>().FirstOrDefault();
                            if (defendingGoalkeeper != null)
                            {
                                defendingGoalkeeper.Play();
                                defendingGoalkeeper.Score();
                                LogMatchEvent($"Спасение: {defendingTeam.Name} - {defendingGoalkeeper.Name}");
                            }
                        }
                    }
                }

                // Имитация времени между минутами
                await Task.Delay(0); // Задержка 1 секунда
            }
        }

        // Перегруженный метод для использования длительности по умолчанию (45 минут)
        private async Task SimulateHalf(Team<Player> attackingTeam, Team<Player> defendingTeam)
        {
            await SimulateHalfAsync(attackingTeam, defendingTeam, 45);
        }

        private async Task SimulateOvertime()
        {
            LogMatchEvent("Начало овертайма: 1");
            // Овертайм 1
            await SimulateHalfAsync(FirstTeam, SecondTeam, 15);

            LogMatchEvent("Перерыв в овертайме");

            LogMatchEvent("Начало овертайма: 2");
            // Овертайм 2
            await SimulateHalfAsync(FirstTeam, SecondTeam, 15);
        }
    }

    public class Tournament
    {
        // Список матчей в турнире
        public List<FootballMatch> Matches { get; private set; }

        // Список команд, участвующих в турнире
        public List<Team<Player>> Teams { get; private set; }

        // Словарь, хранящий количество очков для каждой команды
        public Dictionary<Team<Player>, int> TeamPoints { get; private set; }

        // Список комментаторов в турнире
        public List<Commentator> Commentators { get; private set; }
        private int currentCommentatorIndex;

        // Список журналистов в турнире
        public List<SportsJournalist> Journalists { get; private set; }
        private int currentJournalistIndex;

        // Список судей в турнире
        public List<Referee> Referees { get; private set; }
        private int currentRefereeIndex;

        // Конструктор класса Tournament
        public Tournament(List<Team<Player>> teams, List<Commentator> commentators, List<SportsJournalist> journalists, List<Referee> referees)
        {
            Teams = teams;
            Commentators = commentators;
            Journalists = journalists;
            Referees = referees;
            currentCommentatorIndex = 0;
            currentRefereeIndex = 0;
            Matches = GenerateMatches(teams, commentators);
            TeamPoints = InitializeTeamPoints(teams);
        }

        // Счетчик переигровок
        private int replayCount = 0;

        // Метод для симуляции турнира
        public async Task SimulateTournament()
        {
            // Проверка наличия минимального количества команд для проведения турнира
            if (Teams.Count < 2)
            {
                throw new FootballException("Недостаточное количество команд для проведения турнира.");
            }

            // Симуляция основных матчей
            foreach (var match in Matches)
            {
                await match.SimulateMatch();
                UpdateTeamPoints(match);
            }

            // Проверка наличия команд с одинаковым количеством очков
            var teamsWithEqualPoints = Teams.GroupBy(t => TeamPoints[t])
                                             .Where(g => g.Count() > 1)
                                             .SelectMany(g => g)
                                             .ToList();

            // Если есть команды с одинаковым количеством очков, проводим переигровки
            if (teamsWithEqualPoints.Any())
            {
                Console.WriteLine("\nКоманды с одинаковым количеством очков. Назначаются переигровки.");

                foreach (var team1 in teamsWithEqualPoints)
                {
                    foreach (var team2 in teamsWithEqualPoints)
                    {
                        if (team1 != team2)
                        {
                            await SimulateReplay(team1, team2);
                        }
                    }
                }
            }

            // Вывод итогов турнира
            Console.WriteLine("\nИтоги турнира:");
            foreach (var team in Teams.OrderByDescending(t => TeamPoints[t]))
            {
                Console.WriteLine($"{team.Name}: Очки - {TeamPoints[team]}");
            }
        }

        // Генерация списка матчей для турнира
        private List<FootballMatch> GenerateMatches(List<Team<Player>> teams, List<Commentator> commentators)
        {
            var matches = new List<FootballMatch>();

            // Создание пар матчей между командами
            for (int i = 0; i < teams.Count - 1; i++)
            {
                for (int j = i + 1; j < teams.Count; j++)
                {
                    var match = new FootballMatch(teams[i], teams[j], GetNextCommentator(), GetNextReferee(), GetNextJournalist());
                    matches.Add(match);
                }
            }

            return matches;
        }

        // Симуляция переигровки между двумя командами
        private async Task SimulateReplay(Team<Player> team1, Team<Player> team2)
        {
            Console.WriteLine($"\nПереигровка между командами {team1.Name} и {team2.Name}!");

            // Создание нового матча для переигровки с новыми командами
            var replayMatch = new FootballMatch(team1, team2, GetNextCommentator(), GetNextReferee(), GetNextJournalist());
            await replayMatch.SimulateMatch();

            // Обновление очков после переигровки
            UpdateTeamPoints(replayMatch);

            // Вывод итогов переигровки
            Console.WriteLine($"Итоги переигровки: {team1.Name} {replayMatch.FirstTeamScore} : {team2.Name} {replayMatch.SecondTeamScore}");
        }

        // Получение следующего комментатора
        private Commentator GetNextCommentator()
        {
            Commentator nextCommentator = Commentators[currentCommentatorIndex];
            currentCommentatorIndex = (currentCommentatorIndex + 1) % Commentators.Count; // Переход к следующему комментатору по кругу
            return nextCommentator;
        }

        // Получение следующего судьи
        private Referee GetNextReferee()
        {
            Referee nextReferee = Referees[currentRefereeIndex];
            currentRefereeIndex = (currentRefereeIndex + 1) % Referees.Count; // Переход к следующему судье по кругу
            return nextReferee;
        }

        // Получение следующего журналиста
        private SportsJournalist GetNextJournalist()
        {
            SportsJournalist nextJournalist = Journalists[currentJournalistIndex];
            currentRefereeIndex = (currentRefereeIndex + 1) % Journalists.Count; // Переход к следующему журналисту по кругу
            return nextJournalist;
        }

        // Инициализация начальных очков для каждой команды
        private Dictionary<Team<Player>, int> InitializeTeamPoints(List<Team<Player>> teams)
        {
            var teamPoints = new Dictionary<Team<Player>, int>();
            foreach (var team in teams)
            {
                teamPoints[team] = 0;
            }
            return teamPoints;
        }

        // Обновление очков команд после завершения матча
        private void UpdateTeamPoints(FootballMatch match)
        {
            if (match.FirstTeamScore > match.SecondTeamScore)
            {
                TeamPoints[match.FirstTeam] += 3; // Победа первой команды
            }
            else if (match.FirstTeamScore < match.SecondTeamScore)
            {
                TeamPoints[match.SecondTeam] += 3; // Победа второй команды
            }
            else
            {
                // Ничья
                TeamPoints[match.FirstTeam] += 1;
                TeamPoints[match.SecondTeam] += 1;
            }
        }
    }

    // Интерфейс для сериализации и десериализации
    public interface IDataSerializer<T>
    {
        void Serialize(string filePath, T data);
        T Deserialize(string filePath);
    }

    // Реализация интерфейса для формата XML
    public class XmlDataSerializer<T> : IDataSerializer<T>
    {
        public void Serialize(string filePath, T data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            using (XmlWriter writer = XmlWriter.Create(fs, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, data);
            }
        }

        public T Deserialize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл не найден.", filePath);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            using (XmlReader reader = XmlReader.Create(fs))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }

    public class JsonDataSerializer<T> : IDataSerializer<T>
    {
        public void Serialize(string filePath, T data)
        {
            string jsonString = JsonSerializer.Serialize(data);
            File.WriteAllText(filePath, jsonString);
        }

        public T Deserialize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл не найден.", filePath);
            }
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }

    public class FootballException : Exception
    {
        // Конструктор класса исключения
        public FootballException(string message) : base(message)
        {
        }
    }

    class Program
    {
        static async Task Main()
        {
            try
            {
                IDataSerializer<Team<Player>> xmlDataSerializer = new XmlDataSerializer<Team<Player>>();

                // Создаем команды
                var team1 = xmlDataSerializer.Deserialize("teamA.xml");
                team1.Name = "Team A";
                var team2 = xmlDataSerializer.Deserialize("teamB.xml");
                team2.Name = "Team B";
                var team3 = xmlDataSerializer.Deserialize("teamC.xml");
                team3.Name = "Team C";

                // Создаем комментаторов
                var commentator1 = new Commentator("Commentator1");
                var commentator2 = new Commentator("Commentator2");

                //Создаем журналистов
                var journalist1 = new SportsJournalist("Journalist1", "log.txt");
                var journalist2 = new SportsJournalist("Journalist2", "log2.txt");

                // Создаем судей
                var referee1 = new Referee("Referee1");
                var referee2 = new Referee("Referee2");

                // Создаем турнир
                var tournament = new Tournament(
                    new List<Team<Player>> { team1, team2, team3 },
                    new List<Commentator> { commentator1, commentator2 },
                    new List<SportsJournalist> { journalist1, journalist2 },
                    new List<Referee> { referee1, referee2 } // Передаем список судей
                );

                // Симулируем турнир
                await tournament.SimulateTournament();

                SortAndSerializeTeams(tournament.Teams);

                Console.ReadLine();
            }
            catch (FootballException ex)
            {
                Console.WriteLine($"Произошло пользовательское исключение: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошло системное исключение: {ex.Message}");
            }
        }

        static void SortAndSerializeTeams(List<Team<Player>> teams)
        {
            var sortedTeams = teams.ToList();
            
            // Сортировка и сериализация команд
            foreach (var team in sortedTeams)
            {
                Console.WriteLine($"Начало сортировки команды {team.Name}");

                // Сортировка основного состава по индивидуальным очкам игроков (асинхронно)
                team.SortStartingPlayersAsync(players => players.Sort((p1, p2) => p2.Scores.CompareTo(p1.Scores))).Wait();

                Console.WriteLine($"Конец сортировки основного состава команды {team.Name}");

                // Сортировка запасных игроков по индивидуальным очкам игроков (асинхронно)
                team.SortSubstitutePlayersAsync(players => players.Sort((p1, p2) => p2.Scores.CompareTo(p1.Scores))).Wait();

                Console.WriteLine($"Конец сортировки запасных игроков команды {team.Name}");
            }

            // Сериализация в XML
            IDataSerializer<List<Team<Player>>> xmlDataSerializer = new XmlDataSerializer<List<Team<Player>>>();
            xmlDataSerializer.Serialize("sortedTeams.xml", sortedTeams);

            // Сериализация в JSON
            IDataSerializer<List<Team<Player>>> jsonDataSerializer = new JsonDataSerializer<List<Team<Player>>>();
            jsonDataSerializer.Serialize("sortedTeams.json", sortedTeams);
        }
    }
}