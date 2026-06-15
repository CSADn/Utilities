using DataTables.Extensions;
using Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataTables
{
    public class Manager<T>
    {
        private HttpRequest _request;
        private Func<List<T>> _entities;
        private InputSettings<T> _settings;

        public Manager(HttpRequest request, Func<List<T>> entities)
        {
            _request = request;
            _entities = entities;

            Initialize();
        }

        private void Initialize()
        {
            var draw = "draw".FromQueryString<int>(notFoundException: true);
            var start = "start".FromQueryString<int>(notFoundException: true);
            var length = "length".FromQueryString<int>(notFoundException: true);
            var search = "search[value]".FromQueryString<string>(notFoundException: true);
            var orderColumnIndex = "order[0][column]".FromQueryString(default(int?));
            var orderColumn = (orderColumnIndex.HasValue
                ? $"columns[{orderColumnIndex}][data]".FromQueryString(default(string))
                : null
            );
            var orderDir = "order[0][dir]".FromQueryString(default(string));

            _settings = new DataTables.InputSettings<T>
            {
                draw = draw,
                start = start,
                length = length,
                search = search,
                orderColumn = orderColumn,
                orderDir = (string.IsNullOrWhiteSpace(orderDir)
                    ? default(OrderDirection?)
                    : (orderDir == "asc"
                        ? DataTables.OrderDirection.Ascending
                        : DataTables.OrderDirection.Descending
                    )
                ),
                entities = _entities
            };

        }

        public Results<T> Process(bool recursive = false, Func<T, string, bool> filter = null)
        {
            var records = _settings.entities.Invoke();
            var total = records.Count();
            var filtered = records.Count();

            if (!string.IsNullOrWhiteSpace(_settings.search))
            {
                if (filter != null)
                    records = records
                        .Where(i => filter(i, _settings.search))
                        .ToList();
                else
                    records = records
                        .Filter(_settings.search, recursive)
                        .ToList();

                filtered = records.Count();
            }

            if (_settings.length > 0)
            {
                records = records
                .Order(
                    _settings.orderColumn,
                    _settings.orderDir
                )
                .Skip(_settings.start)
                .Take(_settings.length)
                .ToList();
            }
            else
            {
                records = records
                   .Order(
                       _settings.orderColumn,
                       _settings.orderDir
                   )
                   .Skip(_settings.start)
                   .ToList();
            }

            return new Results<T>
            {
                draw = _settings.draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data = records
            };
        }
    }
}
