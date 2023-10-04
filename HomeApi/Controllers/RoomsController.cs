using System.Threading.Tasks;
using AutoMapper;
using HomeApi.Contracts.Models.Rooms;
using HomeApi.Data.Models;
using HomeApi.Data.Repos;
using Microsoft.AspNetCore.Mvc;

namespace HomeApi.Controllers
{
    /// <summary>
    /// Контроллер комнат
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class RoomsController : ControllerBase
    {
        private IRoomRepository _repository;
        private IMapper _mapper;
        
        public RoomsController(IRoomRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _repository.GetAllRooms();
            var responce = new GetRoomsResponse()
            {
                RoomAmount = rooms.Length,
                Rooms = _mapper.Map<Room[], RoomView[]>(rooms)
            };

            return Ok(responce);
        }

        /// <summary>
        /// Добавление комнаты
        /// </summary>
        [HttpPost] 
        [Route("")] 
        public async Task<IActionResult> Add([FromBody] AddRoomRequest request)
        {
            var existingRoom = await _repository.GetRoomByName(request.Name);
            if (existingRoom == null)
            {
                var newRoom = _mapper.Map<AddRoomRequest, Room>(request);
                await _repository.AddRoom(newRoom);
                return StatusCode(201, $"Комната {request.Name} добавлена!");
            }
            
            return StatusCode(409, $"Ошибка: Комната {request.Name} уже существует.");
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Edit([FromQuery]string name, [FromBody] EditRoomRequest request)
        {
            var room = await _repository.GetRoomByName(name);
            if (room == null)
                return BadRequest($"Комната с названием {name} не существует");

            if (!string.IsNullOrEmpty(request.NewName))
            {
                var existingRoom = await _repository.GetRoomByName(request.NewName);
                if (existingRoom != null)
                    return BadRequest($"Комната с названием {request.NewName} уже существует. Укажите другое название комнаты.");
                room.Name = request.NewName;
            }

            if (request.NewArea != null)
                room.Area = request.NewArea.Value;

            if (request.NewGasConnected != null)
                room.GasConnected = request.NewGasConnected.Value;

            if (request.NewVoltage != null)
                room.Voltage = request.NewVoltage.Value;

            await _repository.UpdateRoom(room);
            return Ok("Комната обновлена!");
        }
    }
}