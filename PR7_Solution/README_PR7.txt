МДК 02.02 — ПР7. Исаев Иван Андреевич, группа И13-24-11, вариант №6.

ЭТА ПАПКА — САМОСТОЯТЕЛЬНАЯ КОПИЯ ПРОЕКТА. НЕ ЗАМЕНЯЙТЕ ЕЮ СТАРУЮ ПАПКУ src И НЕ УДАЛЯЙТЕ .git!
1. Распакуйте в C:\Users\Иван\Desktop\MDK6\PR7_Solution (или другой путь).
2. ОСТАНОВИТЕ старый сервер CorporateCarsharing, чтобы освободить порт 57000.
3. Откройте CorporateCarsharing.PR7.sln в Visual Studio (Windows, .NET 8 и WPF workload).
4. Сборка: Ctrl+Shift+B. Если не удастся восстановить пакеты, проверьте NuGet/internet.
5. В обозревателе решений правой кнопкой CorporateCarsharing.Api -> Назначить запускаемым проектом. F5.
6. Swagger: https://localhost:57000/swagger. GET /api/v1/vehicles, POST /api/v1/bookings, GET /api/v1/version.
7. Тесты: Тест -> Обозреватель тестов -> Запустить все (возможна загрузка NuGet пакетов).
8. Для UI: остановите API? Нет: откройте второй экземпляр Visual Studio с этим решением, запустите в нём CorporateCarsharing.UI, сохраняя сервер запущенным в первом окне.
9. Пришлите скриншоты сборки, структуры 5 проектов, результатов тестов.

Архитектура:
Core: доменные модели, интерфейс хранилища, бизнес-правила BookingService/TelemetryMappingService.
Infrastructure: хранилище в оперативной памяти с реализацией ICarsharingRepository.
Api: HTTP/Swagger и компоновка зависимостей.
UI: WPF MVVM-клиент из ПР6 (общение с API по HTTPS, не прямая ссылка на Core).
Tests: xUnit (зависимости от Core и Infrastructure).
Зависимости компиляции: Infrastructure -> Core; Api -> Core + Infrastructure; Tests -> Core + Infrastructure; UI -> HTTP -> Api (во время работы).

Ограничения: БД нет; хранение в памяти; авторизация не реализована. Успешная сборка этого нового решения ещё НЕ ПОДТВЕРЖДЕНА: проверьте на своём ПК.
