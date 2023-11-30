#include "pch.h"
#include "mkl.h"
#include <math.h>
#include <iostream>

#include "extras.hpp"

using namespace std;


void SquaredError(MKL_INT* m, MKL_INT* n, double* yEstimated, double* f, void *userData) {
	AdditionalData* data = static_cast<AdditionalData*>(userData);
	double* splineValues = new double[data->nKnownX];

	CubicSpline(
		data->mX, data->leftX, data->rightX, // данные на которых строится сплайн
		yEstimated, // размерность функции и её значения на точках
		data->nKnownX, data->XforEstimation, splineValues // точки в которых нужно посчитать значения и массив для ответов
	);

	
	for (int i = 0; i < data->nKnownX; i++) {
		f[i] = splineValues[i] - data->YforTests[i];
	}
	
	
	delete[] splineValues;
}
