#pragma once

struct AdditionalData {
	int mX;
	double leftX;
	double rightX;

	int nKnownX;
	double* XforEstimation;
	double* YforTests;
};


enum class ErrorEnum { NO, INIT, CHECK, SOLVE, JACOBI, GET, DELETER, RCI };
void SquaredError(MKL_INT* m, MKL_INT* n, double* x, double* f, void* userData);


int CubicSpline(
	int mX,
	double leftX,
	double rightX,

	const double* Y,

	// на чём вычисляются проверочные значения сплайна
	const int nKnownX,
	double* knownX,

	double* splineValues // массив вычисленных значений сплайна и производных
);

typedef void (*FSpline) (
	int mX,
	double leftX,
	double rightX,

	int mY,
	const double* Y,

	// на чём вычисляются проверочные значения сплайна
	const int nKnownX,
	double* knownX,

	double* splineValues
);