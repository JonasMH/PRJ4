/* ========================================
 *
 * Copyright YOUR COMPANY, THE YEAR
 * All Rights Reserved
 * UNPUBLISHED, LICENSED SOFTWARE.
 *
 * CONFIDENTIAL AND PROPRIETARY INFORMATION
 * WHICH IS THE PROPERTY OF your company.
 *
 * ========================================
*/
#include "PID.h"
#include <project.h>

static struct PIDparameter parameter_;

static int PIDval = 0;
static int dt = dt_def;
static int iState = 0;
static int err = 0;
static int pre_err = 0;


void init()
{
    parameter_.Kp = Kp_def;
    parameter_.Ki = Ki_def;
    parameter_.Kd = Kd_def;
    parameter_.MAX = MAX_def;
    parameter_.MIN = MIN_def;
    parameter_.iMAX = iMAX_def;
    parameter_.iMIN = iMIN_def;
    
    PWM_1_Start();
    PWM_1_WriteCompare((unsigned char)parameter_.MIN);
    
}

void tick(int sensor, int input)
{
    PIDval = 0;
	err = input - sensor;
	
	PIDval = parameter_.Kp*err; //Proportional calc.
	
	iState += err*dt;
	if(iState > parameter_.iMAX)
		iState = parameter_.iMAX;
	else if(iState < parameter_.iMIN)
		iState = parameter_.iMIN;
	
	PIDval += parameter_.Ki*iState; //intergral calc
	
	PIDval += parameter_.Kd*((err-pre_err)/dt); //differentiel calc
	
	pre_err = err;
	
	if(PIDval > parameter_.MAX)
		PIDval = parameter_.MAX;
    else if(PIDval < parameter_.MIN)
        PIDval = parameter_.MIN;
	
	PWM_1_WriteCompare((unsigned char)PIDval);
}

void setPID(struct PIDparameter * parameter)
{
    parameter_ = *parameter;
}

/* [] END OF FILE */
