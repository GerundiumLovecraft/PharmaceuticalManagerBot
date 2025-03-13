#PharManager Bot

## Languages
-[Русский](#описание)
-[English](#description)

# Описание

PharManager — телеграм бот для ведения учёта за лекарственными препаратами в домашней аптечке и их сроком годности.

# Установка

1. Клонируйте репозиторий:
    ```bash
    git clone https://github.com/ваш-репозиторий.git
    ```
2. Перейдите в директорию проекта:
    ```bash
    cd ваш-репозиторий
    ```
3. Установите зависимости:
    ```bash
    dotnet restore
    ```
4. Настройте конфигурационные файлы (для своего проекта я использовал secrets.js):
   - токен телеграм бота
   - строка для подключения к PostgreSQL серверу
5. Запуск
   ```bash
   dotnet run
   ```
  
# Использование

Для взаимодействия с ботом используются следующий команды:
  - /start :
    Начинает чат с ботом, добавляет данные о пользователе в базу данных, отправляет пользователю приветственное сообщение.
  - /add :
    Высылает сообщение с инструкцией, в каком формате отправлять данные о препарате, присвоить чату статус "GET_MED_DETAILS", чтобы обработать строку из следующего сообщения.
  - /show :
    Отправляет все препараты пользователя в виде списка, где вид разделён на страницы из 5 препаратов с возможностью навигации при помощи кнопок.
  - /show_types :
    Отправляет список типов лекарственных средств и их порядковых номеров. При добавлении препарата, его тип указывается порядковым номером.
  - /expire_soon :
    Отправляет список лекарственных средств, срок годности которых заканчивается в течение 30 дней.
  - /show_expired :
    Отправляет список лекарственных средств, срок годности которых истёк.
  - /auto_check :
    Включает автоматическую проверку списка препаратов, срок годности которых заканчивается в течение 30 дней.
  - /auto_check_disable :
    Отключает автоматическую проверку списка препаратов, срок годности которых заканчивается в течение 30 дней.
  - /help :
    Отправляет список команд с коротким описанием.

# Description

PharManager is a telegram bot for keeping records of medicines in the home first-aid kit and their expiration date.
The bot communicates in Russian only at the moment, addition of English is in the plans.

# Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/ваш-репозиторий.git
    ```
2. Install dependencies:
    ```bash
    dotnet restore
    ```
3. Setup the secrets:
   - Telegram bot token
   - PostgreSQL connection string
4. Run
   ```bash
   dotnet run
   ```
  
# Usage

To interact with the bot the following commands are used:
  - /start :
    Start a chat with the bot, user details are added to the DB, greeting message is sent to the user.
  - /add :
    Sends an instruction with an example of how the medicine details should be sent to the bot, "GET_MED_DETAILS" status set for the chat to track the next message.
  - /show :
    Sends a list of all user's medications, where a view consists of 5 entries with the ebility to navigate the list with the buttons.
  - /show_types :
    Sends a list of medication types and their IDs. 
  - /expire_soon :
    Sends a list of medications with an expiry date within 30 days.
  - /show_expired :
    Sends a list of expired medications.
  - /auto_check :
    Enables the automatic check for the medications with an expiry date within 30 days.
  - /auto_check_disable :
    Disables the automatic check for the medications with an expiry date within 30 days.
  - /help :
    Sends a list of all commands.
