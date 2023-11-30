#include "pch.h"
#include "mkl.h"

#include <iostream>

using namespace std;
int CubicSpline(
	int mX,
	double leftX,
	double rightX,
	
	const double* yEstimated,
	
	// �� ��� ����������� ����������� �������� �������
	const int nKnownX, 
	double *knownX,

	double* splineValues // ������ ����������� �������� ������� � �����������
)
{
	MKL_INT s_order = DF_PP_CUBIC; // ���������� ������
	MKL_INT s_type = DF_PP_NATURAL; // ������������ ���������� ������
	
	// ��������� ������� �� ������ ����������� �� ����� ������
	MKL_INT bc_type = DF_BC_2ND_LEFT_DER | DF_BC_2ND_RIGHT_DER;
	// ������ ��� ������������� �������
	int mY = 1;
	double* scoeff = new double[mY * (mX - 1) * s_order];
	
	try
	{
		int status = -1;
		
		double grid[2]{ leftX, rightX }; // ������� ����������� �����
		DFTaskPtr task;
		status = dfdNewTask1D(
			&task,
			mX, grid, DF_UNIFORM_PARTITION,
			mY, yEstimated, DF_NO_HINT
		);

		if (status != DF_STATUS_OK) throw 1;
		
		
		double bc[2]{ 0, 0 }; // �������� ������ ����������� �� �������
		status = dfdEditPPSpline1D(task,
			s_order, s_type, bc_type, bc,
			DF_NO_IC, NULL, scoeff, DF_NO_HINT
		);



		if (status != DF_STATUS_OK) throw 2;
		
		// �������� �������
		status = dfdConstruct1D(task, DF_PP_SPLINE, DF_METHOD_STD);

		if (status != DF_STATUS_OK) throw 3;

		// ���������� �������� ������� � ������
		int nDorder = 1;
		MKL_INT dorder[] = { 1 };
		

		status = dfdInterpolate1D(task,
			DF_INTERP, DF_METHOD_PP,
			nKnownX, knownX, DF_NON_UNIFORM_PARTITION,
			nDorder, dorder, NULL,
			splineValues, DF_NO_HINT, NULL
		);

		if (status != DF_STATUS_OK) throw 4;

		// ������������ ��������
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