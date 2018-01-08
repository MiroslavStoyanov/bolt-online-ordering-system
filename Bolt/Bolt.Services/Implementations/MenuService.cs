﻿namespace Bolt.Services.Implementations
{
    using System;
    using System.Threading.Tasks;

    using Contracts;
    using DTOs.Orders;
    using Data.Contexts.Bolt.Core.Repositories;

    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            this._menuRepository = menuRepository;
        }

        //TODO; Implement UnitOFWork here, the repository needs to be caught from there
        public async Task<GetMenuDTO> GetMenuAsync()
        {
            try
            {
                GetMenuDTO menu = await this._menuRepository.GetMenuAsync();
                return menu;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Something went wrong while getting the menu.", ex);
            }
        }
    }
}