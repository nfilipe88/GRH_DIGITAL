import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RealizarAvaliacao } from './realizar-avaliacao';

describe('RealizarAvaliacao', () => {
  let component: RealizarAvaliacao;
  let fixture: ComponentFixture<RealizarAvaliacao>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RealizarAvaliacao]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RealizarAvaliacao);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
