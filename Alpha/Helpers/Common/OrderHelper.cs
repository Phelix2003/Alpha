using Alpha.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;


namespace Alpha.Helpers.Common
{
    public class OrderHelper
    {
        public async Task CreateOrderSlotListForDay(int RestoId, DateTime Date, ApplicationDbContext DbManager )
        {
            // Retrieve the resto to create the slots.
            Resto resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == RestoId);

            if (resto != null)
            {
                if (resto.OrderIntakeSlots.FirstOrDefault(r => r.OrderSlotTime.Date == Date.Date) == null) // Check if slots time for that day has already been created 
                {
                    int i = 1;
                    foreach (var times in resto.OpeningTimes.Where(r => r.DayOfWeek == Date.DayOfWeek))
                    {
                        i = i++;
                        List<TimeSpan> orderSlotList = times.GetListOfOrderSlots();
                        foreach (var slotToConvert in orderSlotList)
                        {
                            DateTime slotInDateTime = new DateTime(Date.Year, Date.Month, Date.Day, slotToConvert.Hours, slotToConvert.Minutes, slotToConvert.Seconds);
                            // Calculate the pediore of the day (Beakefast, Lunch, Diner) depending on the order time; 

                            MealTime mealTime = new MealTime();

                            // Before 11:30 this is breakfast
                            if(TimeSpan.Compare(slotInDateTime.TimeOfDay, new DateTime(2000,1,1,11,30,0).TimeOfDay) < 1)
                            {
                                mealTime = MealTime.Breakfast;
                            }
                            else
                            {
                                //Between 11:30 and 16:00 lunh time
                                if (TimeSpan.Compare(slotInDateTime.TimeOfDay, new DateTime(2000, 1, 1, 16, 00, 0).TimeOfDay) < 1)
                                {
                                    mealTime = MealTime.Lunch;
                                }
                                else
                                {
                                    mealTime = MealTime.Diner;
                                }
                            }
                            resto.OrderIntakeSlots.Add(new OrderSlot { Resto = resto, OrderSlotTime = slotInDateTime, SlotGroup = mealTime });
                        }
                    }
                    await DbManager.SaveChangesAsync();
                }
            }
            else
            {
                throw new Exception("CreateOrderSlotListForDay -- No resto found with RestoId!");
            }
        }

    }
}