import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { OvertimeService } from '../../../../core/services/overtime.service';
import { OvertimeRequest, getStatusLabel, getStatusClass } from '../../../../core/models/overtime.model';

@Component({
    selector: 'app-overtime-detail',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './overtime-detail.component.html',
    styleUrls: ['./overtime-detail.component.scss']
})
export class OvertimeDetailComponent implements OnInit {
    overtimeRequest: OvertimeRequest | null = null;
    loading: boolean = true;
    error: string = '';

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private overtimeService: OvertimeService
    ) { }

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.loadOvertimeRequest(id);
        }
    }

    loadOvertimeRequest(id: string): void {
        this.loading = true;
        this.overtimeService.getOvertimeById(id).subscribe({
            next: (request) => {
                this.overtimeRequest = request;
                this.loading = false;
            },
            error: (error) => {
                console.error('Error loading overtime request:', error);
                this.error = 'Failed to load overtime request';
                this.loading = false;
            }
        });
    }

    getStatusLabel(status: number): string {
        return getStatusLabel(status);
    }

    getStatusClass(status: number): string {
        return getStatusClass(status);
    }

    formatDateTime(date: Date): string {
        return date ? new Date(date).toLocaleString() : '-';
    }

    goBack(): void {
        this.router.navigate(['/requests/overtime']);
    }
}
