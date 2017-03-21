#изменения

изменился способ записи в файл. Теперь записывается сразу в файл каждая отдельная структура из бинарного файла. Результат сообщается пользователю только после обработки всех записей.
При неудаче созданный файл удаляется. При успехе сообщается ссылка на готовый файл.

изменился способ добавления записей в базу данных.
чтение записей из бинарного файла приводит к добавлению сущностей в контекст базы данных. Для оптимизации после каждых N(константа, можно задать) записей, выполняется сохранение в базу. Контекст пересоздается, память очищается.
При неудаче транзакция откатывается.

# Тестовое задание Brokeree Solutions
ссылка на резюме:
http://izhevsk.hh.ru/resume/66168d02ff02c7e1ad0039ed1f546b61494b39

В данном солюшене предоставлен сервис по конвертации бинарных файлов в записи базы данных либо в CSV файл.
В дальнейшем предоставляется возможность достать по одной записи из БД и скачать CSV файл по соответствующей ссылке.

Использована SQL Server Compact 4.0,
Entity Framework,
Web API self host,
NLog для логирования.

#Конвертация:
/api/convert/ метод конвертации ? filename= абсолютный путь к файлу

/api/convert/ метод конвертации/путь к файлу в папке запуска приложения

методы конвертации: db или csv

Пример:
/api/convert/db?filename=c:/BinaryDataFile

/api/convert/db/BinaryDataFile

/api/convert/csv?filename=c:/BinaryDataFile

/api/convert/csv/BinaryDataFile

При конвертации в CSV файл, в ответе придет строка, по которой можно запросить данный файл через этот же API
Пример: 
{
  "UrlAddress": "api/download/6c43f209-76ac-4e85-8cb9-3896624b8a5e"
}

CSV файл сохраняется в той же директории что и исходный файл.
В базу данных добавляется запись о пути к файлу и соответствующем ему Guid. По этому Guid в дальнейшем можно скачать файл.

При конвертации в DB ответом будет 200 код.

В базу данных заносятся соответствующие записи о Trade Records. Не стал реализовывать отдельную таблицу для хедера и связывать ее с записями, поэтому каждая запись хранит в себе информацию о версии хедера и типе.

#Загрузка:
/api/download/Guid строка

Пример:
/api/download/6c43f209-76ac-4e85-8cb9-3896624b8a5e

В ответе придет файл CSV. Можно было реализовать загрузку по названиям файлов, как опцию, либо наоборот.

При успешной передаче файлов файл удаляется и соответствующая запись в бд тоже.

#Получение записи:
/api/record/id/путь к файлу в папке запуска приложения

/api/record/id?filename=абсолютный путь до файла

связка id и имя файла являются ключом, и при конвертаци одно и того же файла вызовет ошибку
пример 
{
  "Message": "Some of the records are already exist in database. It appears you are converting the same file."
}

#Логирование

Файлы логов лежат в директории запуска приложения, в папке logs. директорию логов можно поменять в конфигурации.

#модель данных
для работы с базой данных использовался entity Framework.

база данных это локальный файл расположенный в App_Data папке приложения. при запуске приложения она копируется в папку запуска, таким оразом исходная база данных остается неизменной.
не стал делать работу с постоянной базой, тк для отладки приложения этого хватает.

#что не сделано

Не сделаны юнит тесты из-за нехватки времени. Скорее всего использовал бы NUnit библиотеку.

Возможность указания адреса и порта для селф хостинга из параметров запуска приложения.



