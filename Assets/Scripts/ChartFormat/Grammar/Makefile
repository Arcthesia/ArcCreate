Generated/UniversalAffChartVisitor.cs: UniversalAffChart.g4
	java -jar ../../../Plugins/antlr-4.13.2-complete.jar \
		-Dlanguage=CSharp \
		-visitor \
		-package ArcCreate.ChartFormat.Grammar \
		-o Generated \
		UniversalAffChart.g4

	echo "Cleaned"; \
		rm -f Generated/*.java; \
		rm -f Generated/*.tokens; \
		rm -f Generated/*.tokens.meta; \
		rm -f Generated/*.interp; \
		rm -f Generated/*.interp.meta;