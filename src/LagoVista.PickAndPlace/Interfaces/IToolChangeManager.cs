﻿using LagoVista.GCode.Commands;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IToolChangeManager
    {
        Task<bool> HandleToolChange(ToolChangeCommand cmd);
    }
}
