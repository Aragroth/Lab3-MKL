#include "pch.h"
#include "mkl.h"

#include <iostream>

using namespace std;
int CubicSpline(
	int mX,
	double leftX,
	double rightX,
	
	const double* yEstimated,
	
	// на чём вычисляются проверочные значения сплайна
	const int nKnownX, 
	double *knownX,

	double* splineValues // массив вычисленных значений сплайна и производных
)
{
	MKL_INT s_order = DF_PP_CUBIC; // кубический сплайн
	MKL_INT s_type = DF_PP_NATURAL; // классический кубический сплайн
	
	// граничные условия на вторую производную на обоих концах
	MKL_INT bc_type = DF_BC_2ND_LEFT_DER | DF_BC_2ND_RIGHT_DER;
	// массив для коэффициентов сплайна
	int mY = 1;
	double* scoeff = new double[mY * (mX - 1) * s_order];
	
	try
	{
		int status = -1;
		
		double grid[2]{ leftX, rightX }; // границы равномерной сетки
		DFTaskPtr task;
		status = dfdNewTask1D(
			&task,
			mX, grid, DF_UNIFORM_PARTITION,
			mY, yEstimated, DF_NO_HINT
		);

		if (status != DF_STATUS_OK) throw 1;
		
		
		double bc[2]{ 0, 0 }; // значения вторых производных на границе
		status = dfdEditPPSpline1D(task,
			s_order, s_type, bc_type, bc,
			DF_NO_IC, NULL, scoeff, DF_NO_HINT
		);



		if (status != DF_STATUS_OK) throw 2;
		
		// Создание сплайна
		status = dfdConstruct1D(task, DF_PP_SPLINE, DF_METHOD_STD);

		if (status != DF_STATUS_OK) throw 3;

		// Вычисление значений сплайна в точках
		int nDorder = 1;
		MKL_INT dorder[] = { 1 };
		

		status = dfdInterpolate1D(task,
			DF_INTERP, DF_METHOD_PP,
			nKnownX, knownX, DF_NON_UNIFORM_PARTITION,
			nDorder, dorder, NULL,
			splineValues, DF_NO_HINT, NULL
		);

		if (status != DF_STATUS_OK) throw 4;

		// Освобождение ресурсов
		status = dfDeleteTask(&task);
		if (status != DF_STATUS_OK) throw 6;
	}
	catch (int ret)
	{
		delete[] scoeff;
		return ret;
	}
	delete[] scoeff;
	return 0;
}