#include "pch.h"
#include "CreateDLL.h"

int __stdcall test01(int a) {
	if (a <= 12) {
		int ans = 1;
		for (int i = 2;i <= a;i++) ans *= i;
		return ans;
	}
	return 0;
}

int __stdcall test02(int a, int b) {
	if (a < b) {
		int c = a;
		a = b;
		b = c;
	}
	return a - b;
}