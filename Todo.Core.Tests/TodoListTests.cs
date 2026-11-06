using System;
using System.IO;
using System.Linq;
using Xunit;
using Todo.Core;

namespace Todo.Core.Tests
{
    public class TodoListTests
    {
        [Fact]
        public void Add_IncreasesCount()
        {
            var list = new TodoList();
            list.Add(" task ");
            Assert.Equal(1, list.Count);
            Assert.Equal("task", list.Items.First().Title);
        }

        [Fact]
        public void Remove_ById_Works()
        {
            var list = new TodoList();
            var item = list.Add("a");
            Assert.True(list.Remove(item.Id));
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void Find_ReturnsMatches()
        {
            var list = new TodoList();
            list.Add("Buy milk");
            list.Add("Read book");
            var found = list.Find("buy").ToList();
            Assert.Single(found);
            Assert.Equal("Buy milk", found[0].Title);
        }

        [Fact]
        public void SaveAndLoad_PreservesItemsCorrectly()
        {
            var list = new TodoList();
            var item1 = list.Add("Первая задача");
            var item2 = list.Add("Вторая задача");
            item2.MarkDone();

            var tempPath = Path.Combine(Path.GetTempPath(), $"todo_test_{Guid.NewGuid()}.json");
            try
            {
                list.Save(tempPath);

                var loadedList = new TodoList();
                loadedList.Load(tempPath);

                Assert.Equal(2, loadedList.Count);

                var loaded1 = loadedList.Items.First(i => i.Id == item1.Id);
                Assert.Equal("Первая задача", loaded1.Title);
                Assert.False(loaded1.IsDone);

                var loaded2 = loadedList.Items.First(i => i.Id == item2.Id);
                Assert.Equal("Вторая задача", loaded2.Title);
                Assert.True(loaded2.IsDone);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Fact]
        public void Load_NonExistentFile_ThrowsFileNotFoundException()
        {
            var list = new TodoList();
            Assert.Throws<FileNotFoundException>(() => list.Load("non_existent_file.json"));
        }
    }
}
