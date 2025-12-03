import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListaAvaliacao } from './lista-avaliacao';

describe('ListaAvaliacao', () => {
  let component: ListaAvaliacao;
  let fixture: ComponentFixture<ListaAvaliacao>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListaAvaliacao]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListaAvaliacao);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
