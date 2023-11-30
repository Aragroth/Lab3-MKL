#include "pch.h"

#include <iostream>
#include <string>
#include "mkl.h"

#include "extras.hpp"

extern "C" _declspec(dllexport)
int TrustRegion(
	MKL_INT mX,
	double leftValue,
	double rightValue,
	MKL_INT niter1, // максимальное число итераций

	int nKnownX,
	double* KnownX,
	double* KnownY,
	
	double* minimalNevazka,
	double* resultY,
	int *iterationsMade
)
{
	MKL_INT n = mX; // число независимых переменных
	MKL_INT m = 1; // число компонент векторной функции

	double* yEstimated = new double[mX];
	for (int i = 0; i < mX; i++) {
		yEstimated[i] = 0;
	}

	MKL_INT niter2 = 100; // максимальное число итераций при выборе шага
	MKL_INT ndoneIter = 0;

	double rs = 10;

	const double eps[6] = { 1e-12, 1e-12, 1e-12, 1e-12, 1e-12, 1e-12 }; // ищем до точностьи MSE
	double jac_eps = 1.0E-8; // точность вычисления элементов матрицы Якоби

	double res_initial = 0;
	double res_final = 0; // финальное значение невязки
	MKL_INT stop_criteria; // причина остановки итераций

	MKL_INT checkInfo[4]; // результат проверки корректности данных
	ErrorEnum error = ErrorEnum(ErrorEnum::NO); // информация об ошибке


	_TRNSP_HANDLE_t handle = NULL;

	double* fvec = NULL; // массив значений векторной функции
	double* fjac = NULL; // массив с элементами матрицы Якоби

	error = ErrorEnum(ErrorEnum::NO);
	
	MKL_INT RCI_Request = 0;

	try
	{
		fvec = new double[nKnownX];
		fjac = new double[nKnownX * nKnownX];
		MKL_INT ret = dtrnlsp_init(&handle, &n, &nKnownX, yEstimated, eps, &niter1, &niter2, &rs);

		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::INIT));

		ret = dtrnlsp_check(&handle, &n, &nKnownX, fjac, fvec, eps, checkInfo);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::CHECK));

		while (true)
		{
			AdditionalData data = {
				mX, leftValue, rightValue, nKnownX, KnownX, KnownY
			};
			ret = dtrnlsp_solve(&handle, fvec, fjac, &RCI_Request);


			if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::SOLVE));
			if (RCI_Request == 0) continue;
			else if (RCI_Request == 1) SquaredError(&nKnownX, &n, yEstimated, fvec, &data);
			else if (RCI_Request == 2)
			{
				ret = djacobix(SquaredError, &n, &nKnownX, fjac, yEstimated, &jac_eps, &data);
				if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::JACOBI));
			}
			else if (RCI_Request >= -6 && RCI_Request <= -1) break;
			else throw (ErrorEnum(ErrorEnum::RCI));
		}

		ret = dtrnlsp_get(&handle, &ndoneIter, &stop_criteria,
			&res_initial, &res_final);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::GET));

		*minimalNevazka = res_final;

		ret = dtrnlsp_delete(&handle);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::DELETER));

		// вычисляем значения для известного набора точек X
		CubicSpline(
			mX, leftValue, rightValue,
			yEstimated, // найденные y_m минимизирующие функционал
			nKnownX, KnownX, resultY
		);
		*iterationsMade = ndoneIter;
	}
	catch (ErrorEnum _error) { error = _error; }

	if (fvec != NULL) delete[] fvec;
	if (fjac != NULL) delete[] fjac;

	return RCI_Request;
}