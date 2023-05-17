import { Component as NgComponent, EventEmitter, Input, OnInit, Output } from '@angular/core';
import * as d3 from 'd3';
import { EChartsOption } from 'echarts';
import { forkJoin } from 'rxjs';
import { Questionnaire } from '../models/questionnaire.model';
import { DateService } from '../services/date.service';
import { Date } from '../models/date.model';
import { QuestionnaireService } from '../services/questionnaire.service';
import { QuestionnaireBarChartSettings, Widget } from '../models/widget.model';
import { SurveyProgress } from '../models/survey.model';

@NgComponent({
    selector: 'app-questionnaire-bar-chart',
    templateUrl: './questionnaire.barchart.component.html'
})
export class QuestionnaireBarChartComponent implements OnInit, Widget {

    @Output() loading: EventEmitter<boolean> = new EventEmitter<boolean>();
    @Output() error: EventEmitter<boolean> = new EventEmitter<boolean>();
    @Output() title: EventEmitter<string> = new EventEmitter<string>(true);
    @Output() subtitle: EventEmitter<string> = new EventEmitter<string>();

    private _settings: QuestionnaireBarChartSettings;
    @Input() set settings(s: QuestionnaireBarChartSettings) {
        if (!s) return;
        this._settings = s;
        this.load();
    }

    public chartOptions: EChartsOption;

    public questionnaire: Questionnaire;
    public date: Date;
    public data: SurveyProgress[];

    constructor(
        private questionnaireService: QuestionnaireService,
        private dateService: DateService,
    ) {
    }

    ngOnInit(): void {
    }

    public load(): void {
        if (!this._settings.questionnaireId || !this._settings.dateId) {
            this.loading.emit(false);
            this.error.emit(true);
            return;
        }

        forkJoin({
            progresses: this.questionnaireService.getProgress(this._settings.questionnaireId, this._settings.dateId, this._settings.entityIds),
            questionnaire: this.questionnaireService.get(this._settings.questionnaireId),
            date: this.dateService.get(this._settings.dateId)
        }).subscribe({
            next: response => {

                this.questionnaire = response.questionnaire;
                this.date = response.date;

                this.title.emit(response.questionnaire.name);
                this.subtitle.emit(response.date.name);

                response.progresses.sort((a, b) => a.entity.code === b.entity.code ? 0 : a.entity.code < b.entity.code ? -1 : 1);
                this.data = response.progresses;

                const xCategories: string[] = [];
                const values: number[] = [];
                const formatter = (value: any): string => {
                    return d3.format(`.1f`)(value * 100) + "%";
                };

                response.progresses.forEach(surveyProgress => {
                    xCategories.push(surveyProgress.entity.code);
                    values.push(surveyProgress.applicableQuestions === 0 ? 0 : surveyProgress.completedQuestions / surveyProgress.applicableQuestions);
                });

                this.chartOptions = {
                    grid: {
                        left: 40,
                        right: 10,
                        top: 10,
                        bottom: 50
                    },
                    xAxis: {
                        type: 'category',
                        data: xCategories,
                        axisLabel: {
                            fontSize: 8,
                            rotate: 90,

                        }
                    },
                    yAxis: {
                        type: 'value',
                        axisLabel: {
                            formatter: formatter,
                            fontSize: 8
                        },

                    },
                    series: [{
                        name: `${response.questionnaire.name}: ${response.date.code}`,
                        type: "bar",
                        data: values,
                        colorBy: "data"
                    }],
                    tooltip: {
                        triggerOn: "click",
                        alwaysShowContent: false,
                        textStyle: {
                            fontSize: 10
                        },
                        valueFormatter: formatter
                    }
                } as EChartsOption;

                this.error.emit(false);
                this.loading.emit(false);
            },
            error: () => {
                this.loading.emit(false);
                this.error.emit(true);
            }
        });
    }
}
