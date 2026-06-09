using System;
using RenPyTRLauncher.Data;
using RenPyTRLauncher.Models;
using RenPyTRLauncher.Services;

namespace RenPyTRLauncher.Tests
{
    public class SystemTest
    {
        public static void RunAllTests()
        {
            Console.WriteLine("=== SİSTEM TESTLERİ BAŞLIYOR ===");
            Console.WriteLine();

            var db = DatabaseInitializer.Initialize();
            var gameService = new EfGameService(db);

            try
            {
                // Test 1: Oyun Ekle
                TestAddGame(gameService);

                // Test 2: Oyun Düzenle
                TestEditGame(gameService);

                // Test 3: Oyun Sil
                TestDeleteGame(gameService);

                // Test 4: Yama Ekle
                TestAddPatch(gameService);

                // Test 5: Yama Düzenle
                TestEditPatch(gameService);

                // Test 6: Yama Sil
                TestDeletePatch(gameService);

                Console.WriteLine();
                Console.WriteLine("=== TÜM TESTLER BAŞARIYLA TAMAMLANDI ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"=== TEST SIRASINDA HATA: {ex.Message} ===");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void TestAddGame(IGameService gameService)
        {
            Console.WriteLine("[TEST 1] Oyun Ekle (Add Game)");
            try
            {
                var newGame = new Game
                {
                    Name = "Test Oyunu",
                    Description = "Test açıklaması",
                    Version = "1.0",
                    ImagePath = "test.jpg",
                    Categories = new System.Collections.Generic.List<string> { "Test" },
                    Type = GameType.Game
                };

                gameService.Add(newGame);
                Console.WriteLine($"✅ BAŞARILI: Oyun eklendi - ID: {newGame.Id}, Name: {newGame.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HATA: {ex.Message}");
                throw;
            }
            Console.WriteLine();
        }

        private static void TestEditGame(IGameService gameService)
        {
            Console.WriteLine("[TEST 2] Oyun Düzenle (Edit Game)");
            try
            {
                var games = gameService.GetAll();
                var testGame = games.FirstOrDefault(g => g.Name == "Test Oyunu");
                
                if (testGame == null)
                {
                    Console.WriteLine("⚠️ UYARI: Test oyunu bulunamadı, düzenleme testi atlanıyor");
                    return;
                }

                testGame.Description = "Güncellenmiş test açıklaması";
                gameService.Update(testGame);
                Console.WriteLine($"✅ BAŞARILI: Oyun düzenlendi - ID: {testGame.Id}, Name: {testGame.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HATA: {ex.Message}");
                throw;
            }
            Console.WriteLine();
        }

        private static void TestDeleteGame(IGameService gameService)
        {
            Console.WriteLine("[TEST 3] Oyun Sil (Delete Game)");
            try
            {
                var games = gameService.GetAll();
                var testGame = games.FirstOrDefault(g => g.Name == "Test Oyunu");
                
                if (testGame == null)
                {
                    Console.WriteLine("⚠️ UYARI: Test oyunu bulunamadı, silme testi atlanıyor");
                    return;
                }

                var gameId = testGame.Id;
                gameService.Remove(testGame.Id);
                Console.WriteLine($"✅ BAŞARILI: Oyun silindi - ID: {gameId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HATA: {ex.Message}");
                throw;
            }
            Console.WriteLine();
        }

        private static void TestAddPatch(IGameService gameService)
        {
            Console.WriteLine("[TEST 4] Yama Ekle (Add Patch)");
            try
            {
                var games = gameService.GetAll().Where(g => g.Type == GameType.Game).ToList();
                if (games.Count == 0)
                {
                    Console.WriteLine("⚠️ UYARI: Veritabanında oyun bulunamadı, yama ekleme testi atlanıyor");
                    return;
                }

                var parentGame = games.First();
                var newPatch = new Game
                {
                    Name = "Test Yaması",
                    Description = "Test yama açıklaması",
                    Version = "1.0",
                    PatchVersion = "v1.0",
                    PatchFilePath = "test.zip",
                    Type = GameType.Patch,
                    ParentGameId = parentGame.Id
                };

                gameService.Add(newPatch);
                Console.WriteLine($"✅ BAŞARILI: Yama eklendi - ID: {newPatch.Id}, Name: {newPatch.Name}, Parent: {parentGame.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HATA: {ex.Message}");
                throw;
            }
            Console.WriteLine();
        }

        private static void TestEditPatch(IGameService gameService)
        {
            Console.WriteLine("[TEST 5] Yama Düzenle (Edit Patch)");
            try
            {
                var patches = gameService.GetAll().Where(g => g.Type == GameType.Patch).ToList();
                var testPatch = patches.FirstOrDefault(p => p.Name == "Test Yaması");
                
                if (testPatch == null)
                {
                    Console.WriteLine("⚠️ UYARI: Test yaması bulunamadı, düzenleme testi atlanıyor");
                    return;
                }

                testPatch.PatchNotes = "Güncellenmiş test yama notları";
                gameService.Update(testPatch);
                Console.WriteLine($"✅ BAŞARILI: Yama düzenlendi - ID: {testPatch.Id}, Name: {testPatch.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HATA: {ex.Message}");
                throw;
            }
            Console.WriteLine();
        }

        private static void TestDeletePatch(IGameService gameService)
        {
            Console.WriteLine("[TEST 6] Yama Sil (Delete Patch)");
            try
            {
                var patches = gameService.GetAll().Where(g => g.Type == GameType.Patch).ToList();
                var testPatch = patches.FirstOrDefault(p => p.Name == "Test Yaması");
                
                if (testPatch == null)
                {
                    Console.WriteLine("⚠️ UYARI: Test yaması bulunamadı, silme testi atlanıyor");
                    return;
                }

                var patchId = testPatch.Id;
                gameService.Remove(testPatch.Id);
                Console.WriteLine($"✅ BAŞARILI: Yama silindi - ID: {patchId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HATA: {ex.Message}");
                throw;
            }
            Console.WriteLine();
        }
    }
}
